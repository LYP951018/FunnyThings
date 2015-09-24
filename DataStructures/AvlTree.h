#pragma once

#include <cstdint>
#include <type_traits>
#include <algorithm>
#include <vector>
#include "..\BasicLib\Assert.h"

template<typename ObjectT>
struct AvlTreeNode
{
	using NodeType = AvlTreeNode<ObjectT>;
	using NodePointer = NodeType*;
	using ConstPointer = const NodeType*;
	using HeightType = std::uint16_t;
	using BalanceFactorType = std::make_signed_t<HeightType>;

	NodePointer leftChild_, rightChild_;
	HeightType height_;
	ObjectT value_;

	static NodePointer& LeftChild(NodePointer p)
	{
		return p->leftChild_;
	}

	static NodePointer LeftChild(ConstPointer p)
	{
		return p->leftChild_;
	}

	static NodePointer& RightChild(NodePointer p)
	{
		return p->rightChild_;
	}

	static NodePointer RightChild(ConstPointer p)
	{
		return p->rightChild_;
	}

	static HeightType& Height(NodePointer p)
	{
		return p->height_;
	}

	static HeightType Height(ConstPointer p)
	{
		return p->height_;
	}

	static ObjectT& Value(NodePointer p)
	{
		return p->value_;
	}

	static const ObjectT& Value(ConstPointer p)
	{
		return p->value_;
	}

	template<typename... Args>
	static NodePointer AllocateNode(Args&&... args)
	{
		auto node = static_cast<NodePointer>(::operator new(sizeof(NodeType)));
		new (&Value(node)) ObjectT(std::forward<Args>(args)...);
		new (&Height(node)) HeightType(1);
		new (&LeftChild(node)) NodePointer();
		new (&RightChild(node)) NodePointer();
		return node;
	}
};

template<typename ObjectT>
class AvlTree
{
public:
	using NodeType = AvlTreeNode<ObjectT>;
	using NodePointer = NodeType*;
	using ConstNodePointer = typename NodeType::ConstPointer;
	using TreeType = AvlTree<ObjectT>;
	using HeightType = typename NodeType::HeightType;
	using BalanceFactorType = typename NodeType::BalanceFactorType;
	using ValueType = ObjectT;

	static_assert(!std::is_same<ObjectT, NodePointer>::value, "NodePointer has the same type with ObjectT");

	AvlTree() noexcept = default;

	AvlTree(const AvlTree& rhs)
	{
		treeHead_ = CopyEntireTree(rhs.treeHead_);
	}

	AvlTree(AvlTree&& rhs) noexcept
		:treeHead_(rhs.treeHead_)
	{
		rhs.treeHead_ = nullptr;
	}

	AvlTree& operator=(const AvlTree& rhs)
	{
		AvlTree(rhs).Swap(*this);
		return *this;
	}

	AvlTree& operator=(AvlTree&& rhs) noexcept
	{
		AvlTree(std::move(rhs)).Swap(*this);
		return *this;
	}

	~AvlTree()
	{
		if (treeHead_ != nullptr)
		{
			std::vector<NodePointer> npStack;
			npStack.push_back(treeHead_);
			while (!npStack.empty())
			{
				auto p = npStack.back();
				npStack.pop_back();
				auto pl = NodeType::LeftChild(p);
				if(pl != nullptr)	npStack.push_back(pl);
				pl = NodeType::RightChild(p);
				if (pl != nullptr)	npStack.push_back(pl);
				delete p;
			}
		}
	}

	void Insert(const ValueType& v)
	{
		treeHead_ = InsertImp(treeHead_, v);
	}

	void Insert(std::initializer_list<ValueType> il)
	{
		for (auto&& v : il)
		{
			Insert(v);
		}
	}

	void Remove(const ValueType& v)
	{
		assert(treeHead_ != nullptr);
		treeHead_ = RemoveImp(treeHead_, v);
	}

	void Swap(AvlTree& rhs) noexcept
	{
		std::swap(treeHead_, rhs.treeHead_);
	}
	
private:

	NodePointer Remove(NodePointer p)
	{
		auto l = NodeType::LeftChild(p);
		auto r = NodeType::RightChild(p);
		delete p;
		if (r == nullptr) return l;
		auto minNode = GetMininum(r);
		NodeType::RightChild(minNode) = RemoveMin(r);
		NodeType::LeftChild(minNode) = l;
		return Balance(minNode);
	}

	NodePointer RemoveImp(NodePointer p, const ValueType& v)
	{
		if (p == nullptr)
		{
			YPASSERT(false, "Nothing found!");
			return nullptr;
		}
		if (v < NodeType::Value(p)) NodeType::LeftChild(p) = RemoveImp(NodeType::LeftChild(p), v);
		else if (v > NodeType::Value(p)) NodeType::RightChild(p) = RemoveImp(NodeType::RightChild(p), v);
		else
		{
			return Remove(p);
		}
		return Balance(p);
	}

	static NodePointer RemoveMin(NodePointer p)
	{
		if (NodeType::LeftChild(p) == nullptr) return NodeType::RightChild(p);
		NodeType::LeftChild(p) = RemoveMin(NodeType::LeftChild(p));
		return Balance(p);
	}

	static NodePointer InsertImp(NodePointer p, const ValueType& v)
	{
		if (p == nullptr) return NodeType::AllocateNode(v);
		if (v < NodeType::Value(p)) 
			NodeType::LeftChild(p) = InsertImp(NodeType::LeftChild(p), v);
		else 
			NodeType::RightChild(p) = InsertImp(NodeType::RightChild(p), v);
		return Balance(p);
	}

	static HeightType GetHeight(ConstNodePointer p)
	{
		return p ? NodeType::Height(p) : 0;
	}

	static BalanceFactorType GetBalanceFactor(ConstNodePointer p)
	{
		return GetHeight(NodeType::RightChild(p)) - GetHeight(NodeType::LeftChild(p));
	}

	static void FixHeight(NodePointer p)
	{
		NodeType::Height(p) =
			std::max(GetHeight(NodeType::LeftChild(p)), GetHeight(NodeType::RightChild(p))) + 1;
	}

	static NodePointer RotateRight(NodePointer p)
	{
		auto q = NodeType::LeftChild(p);
		auto ql = NodeType::RightChild(q);
		NodeType::LeftChild(p) = ql;
		NodeType::RightChild(q) = p;
		FixHeight(p);
		FixHeight(q);
		return q;
	}

	static NodePointer RotateLeft(NodePointer q)
	{
		auto p = NodeType::RightChild(q);
		auto pl = NodeType::LeftChild(p);
		NodeType::RightChild(q) = pl;
		NodeType::LeftChild(p) = q;
		FixHeight(q);
		FixHeight(p);
		return p;
	}

	static NodePointer Balance(NodePointer p)
	{
		FixHeight(p);
		auto balanceFactor = GetBalanceFactor(p);
		if (balanceFactor == 2)
		{
			//right subtree is higher
			auto pr = NodeType::RightChild(p);
			if (GetBalanceFactor(pr) < 0)
				NodeType::RightChild(p) = RotateRight(pr);
			return RotateLeft(p);
		}
		else if (balanceFactor == -2)
		{
			auto pl = NodeType::LeftChild(p);
			if (GetBalanceFactor(pl) > 0)
				NodeType::LeftChild(p) = RotateLeft(pl);
			return RotateRight(p);
		}
		return p;
	}

	static NodePointer GetMininum(NodePointer p)
	{
		assert(p != nullptr);
		NodePointer temp = nullptr;
		while ((temp = NodeType::LeftChild(p)) != nullptr)
		{
			p = temp;
		}
		return p;
	}

	static NodePointer CopyEntireTree(ConstNodePointer root)
	{
		if (root == nullptr) return nullptr;
		auto node = NodeType::AllocateNode(NodeType::Value(root));
		NodeType::LeftChild(node) = CopyEntireTree(NodeType::LeftChild(root));
		NodeType::RightChild(node) = CopyEntireTree(NodeType::RightChild(root));
		return node;
	}

	NodePointer treeHead_ = nullptr;
};

template<typename ObjectT>
void swap(AvlTree<ObjectT>& lhs, AvlTree<ObjectT>& rhs) noexcept
{
	lhs.Swap(rhs);
}
