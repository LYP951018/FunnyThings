#pragma once

#include <cstdint>
#include <new> //for ::operator new
#include "..\BasicLib\Assert.h"

//http://eternallyconfuzzled.com/tuts/datastructures/jsw_tut_rbtree.aspx

template<typename ObjectT>
struct RBTreeNode
{
	using NodeType = RBTreeNode;
	using ValueType = ObjectT/*int*/;
	using Pointer = NodeType*;
	using ConstPointer = const NodeType*;

	//0 indicates left
	//1 indicates right
	Pointer links_[2];
	ValueType data_;
	bool isRed_;
};

template<typename ObjectT>
class RBTree
{
public:
	using NodeType = RBTreeNode<ObjectT>;
	using NodePointer = typename NodeType::Pointer;
	using ConstNodePointer = typename NodeType::ConstPointer;
	using ValueType = typename NodeType::ValueType;

	RBTree() noexcept = default;

	~RBTree()
	{
		Tidy(treeHead_);
	}

	RBTree(const RBTree& rhs)
	{
		treeHead_ = CopyTree(rhs.treeHead_);
	}

	RBTree(RBTree&& rhs) noexcept
		:treeHead_(rhs.treeHead_)
	{
		rhs.treeHead_ = nullptr;
	}

	RBTree& operator= (const RBTree& rhs)
	{
		RBTree(rhs).swap(*this);
		return *this;
	}

	RBTree& operator= (RBTree&& rhs) noexcept
	{
		RBTree(std::move(rhs)).swap(*this);
		return *this;
	}

	void Insert(std::initializer_list<ValueType> il)
	{
		for (auto&& v : il)
			Insert(v);
	}


	void Insert(const ValueType& v)
	{
		if (treeHead_ == nullptr)
		{
			treeHead_ = AllocateNode(v);
		}
		else
		{
			NodeType dummyNode{};

			NodePointer grandNode, grandNodeFather;
			NodePointer parent, cur;
			std::uint8_t dir = 0, last;
			grandNodeFather = &dummyNode;
			grandNode = parent = nullptr;
			cur = grandNodeFather->links_[1] = treeHead_; 

			while (true)
			{
				if (cur == nullptr)
				{
					parent->links_[dir] = cur = AllocateNode(v);
				}
				else if (IsRed(cur->links_[0]) && IsRed(cur->links_[1]))
				{
					//Color Flip
					//  1,E <--parent            1,E
					//	 \                         \
					//   3,B <--cur      ->        3,R					
					//   / \                       / \
					// 2,R 4,R                   2,B 4,B

					cur->isRed_ = true;
					cur->links_[0]->isRed_ = false;
					cur->links_[1]->isRed_ = false;
				}

				//red violation
				//parent is the parent of cur
				//For the color flip case, we test the current node's children.
				//If they're red then the current node must be black, so we do a color flip. 
				//This could cause a red violation further up the tree (which is why we saved the parent of the current node). 
				
				if (IsRed(cur) && IsRed(parent))
				{
					//Now, because a color flip could immediately cause a violation, 
					//we go straight to the violation test between the parent and the current node. If both are red, we do a single or double rotation at the grandparent:
					auto dir2 = grandNodeFather->links_[1] == grandNode;

					if (cur == parent->links_[last])
					{
						//	Single Rotation
						//     0,B <--g                 1,B
						//       \                      / \
						//       1,R <--p              0,R 3,R
						//         \          ->            / \
						//         3,R <--q               2,B 4,B
						//         / \
						//       2,B 4,B
						grandNodeFather->links_[dir2] = SingleRotate(grandNode, !last);
					}
					else
					{

					//	Double Rotation
					//      0,B <--g                 2,B
					//        \                      / \
					//        4,R <--p             0,R 4,R
					//        /             ->       \  /
					//       2,R <--q                1,B 3,B
					//      / \
					//    1,B 3,B
						grandNodeFather->links_[dir2] = DoubleRotate(grandNode, !last);
					}
				}

				//TODO: remove this 
				if (cur->data_ == v) break;

				//last is the relationship between grandNode and parent (left or right)
				last = dir;

				//dir is the relationship between cur and parent 
				dir = cur->data_ < v;

				if (grandNode != nullptr) grandNodeFather = grandNode;

				grandNode = parent;
				parent = cur;
				cur = parent->links_[dir];
			}
			treeHead_ = dummyNode.links_[1];
		}
		treeHead_->isRed_ = false;
#ifdef _DEBUG
		RBTreeTest(treeHead_);
#endif
	}

	bool Remove(const ValueType& v)
	{
		YPASSERT(treeHead_ != nullptr, "You can't remove nodes from an empty tree.");
		if (treeHead_ != nullptr)
		{
			NodeType dummyNode{};
			NodePointer cur, parent, grandNode;
			NodePointer nodeToDelete{};
			
			std::uint8_t dir = 1;

			cur = &dummyNode;
			grandNode = parent = nullptr;
			cur->links_[1] = treeHead_;

			while (cur->links_[dir] != nullptr)
			{
				//last last is the relationship between parent and cur.
				auto last = dir;

				grandNode = parent;
				parent = cur;
				cur = cur->links_[dir];
				//dir is the relationship between the next cur and the current cur.
				dir = cur->data_ < v;

				if (cur->data_ == v)
					nodeToDelete = cur;
				if (!IsRed(cur) && !IsRed(cur->links_[dir]))
				{
					if (IsRed(cur->links_[!dir]))
					{
						//   1,B---cur        0,B 
						//	/   \---dir         \
						// 0,R  2,B  ->         1,R----cur
						//                        \
						//                        2,B
						parent = parent->links_[last] = SingleRotate(cur, dir);

						//     0,B ---parent
						//      \
						//      1,R----cur
						//        \
						//        2,B
					}
					else /*if (!IsRed(cur->links_[!dir]))*/
					{
						//	1,R->p          1,B
						//	/ \     ->      / \
						//0,B 2,B->cur    0,R 2,R

						//The first case is a simple reverse color flip. 
						//If a node and it's sibling are black, and all four of their children are black, 
						//make the parent black and both children red

						//the children of cur are both black.
						//s is cur's brother
						auto s = parent->links_[!last];
						if (s != nullptr)
						{
							if (!IsRed(s->links_[!last]) && !IsRed(s->links_[last]))
							{
								parent->isRed_ = false;
								s->isRed_ = true;
								cur->isRed_ = true;
							}
							else
							{
								//However, in this case the color changes of jsw_single won'grandNodeFather quite cut it. 
								//To be thorough, we'll force the correct coloring for all affected nodes:
								auto dir2 = grandNode->links_[1] == parent;
								if (IsRed(s->links_[last]))
								{
									//           2,R----p      1,R
									//      	/ \----last	   / \
									// s----  0,B 3,B-cur  ->    0,B  2,B
									//         \                   \
									//         1,R                 3,R
									grandNode->links_[dir2] = DoubleRotate(parent, last);
								}
								else if (IsRed(s->links_[!last]))
								{
									//      	2,R          1,R
									//      	/ \			 / \
									//        1,B  3,B  ->  0,B 2,B
									//       /                \
									//		0,R              3,R
									grandNode->links_[dir2] = SingleRotate(parent, last);
								}
								auto tmp = grandNode->links_[dir2];
								cur->isRed_ = tmp ->isRed_ = true;
								tmp->links_[0] = false;
								tmp->links_[1] = false;
							}
						}
					}
				}
			}
			//push the red node down to cur
			if (nodeToDelete != nullptr)
			{
				nodeToDelete->data_ = std::move(cur->data_);
				parent->links_[parent->links_[1] == cur] = cur->links_[cur->links_[0] == nullptr];
				delete cur;
			}
			else return false;

			if (treeHead_ != nullptr)
				treeHead_->isRed_ = false;
		}
#ifdef _DEBUG
		RBTreeTest(treeHead_);
#endif
		return true;
	}

	void swap(RBTree& rhs) noexcept
	{
		std::swap(treeHead_, rhs.treeHead_);
	}

private:

	static bool IsRed(ConstNodePointer p)
	{
		return p != nullptr && p->isRed_;
	}

	//dir: 0 left 1 right
	static NodePointer SingleRotate(NodePointer p, std::uint8_t dir)
	{
		//       p                          cur
		//		/\							/\
		//     cur C   left <-> right      A cur
		//	   /\                             /\
		//    A  B                            B C

		YPASSERT(dir == 0 || dir == 1, "Invalid direction.");
		auto temp = p->links_[!dir];

		p->links_[!dir] = temp->links_[dir];
		temp->links_[dir] = p;

		p->isRed_ = true;
		temp->isRed_ = false;

		return temp;
	}

	static NodePointer DoubleRotate(NodePointer p, std::uint8_t dir)
	{

		//left:
		//      p			 p              s
		//      /\          /\              /\
		//     A  cur	   A  s            p  cur
		//        /\	->    /\     ->    /\  /\
		//      s D         B  cur        A B C D
		//		/\	            /\
		//	    B C            C  D


		p->links_[!dir] = SingleRotate(p->links_[!dir], !dir);

		return SingleRotate(p, dir);
	}

	static std::uint16_t RBTreeTest(ConstNodePointer root)
	{
		std::uint16_t leftHeight, rightHeight;

		if (root == nullptr) return 1;
		else
		{
			auto ln = root->links_[0];
			auto rn = root->links_[1];

			if (IsRed(root))
			{
				YPASSERT(!IsRed(ln) && !IsRed(rn), "Red node's child is red,too.");
			}

			leftHeight = RBTreeTest(ln);
			rightHeight = RBTreeTest(rn);

			YPASSERT(!(leftHeight != 0 && rightHeight != 0 && leftHeight != rightHeight), "Black heights are not equal.");
			
			if (leftHeight != 0 && rightHeight != 0)
			{
				return IsRed(root) ? leftHeight : leftHeight + 1;
			}
			else return 0;
		}
	}

	template<typename... Args>
	static NodePointer AllocateNode(Args&&... args)
	{
		auto node = static_cast<NodePointer>(::operator new(sizeof(NodeType)));
		new (&node->links_[0]) NodePointer{};
		new (&node->links_[1]) NodePointer{};
		node->isRed_ = true;
		new (&node->data_) ValueType{ std::forward<Args>(args)... };
		return node;
 	}

	void Tidy(NodePointer p)
	{
		if (p == nullptr) return;
		auto l = p->links_[0];
		auto r = p->links_[1];
		delete p;
		Tidy(l);
		Tidy(r);
	}

	 static NodePointer CopyTree(ConstNodePointer p)
	{
		if (p == nullptr) return nullptr;
		auto node = AllocateNode(p->data_);
		node->links_[0] = CopyTree(p->links_[0]);
		node->links_[1] = CopyTree(p->links_[1]);
		return node;
	}

	NodePointer treeHead_ = {};
};

template<typename ObjectT>
void swap(RBTree<ObjectT>& lhs, RBTree<ObjectT>& rhs) noexcept
{
	lhs.swap(rhs);
}