#include "AvlTree.h"
#include "RBTree.h"

int main()
{
	{
		AvlTree<int> tree;
		tree.Insert({ 3,2,1,4,7,4,5,6,10,16,8,9,15,14,13,11 });
		tree.Remove(7);
		auto tree2 = tree;
	}
	{
		RBTree<int> tree1;
		tree1.Insert({ 11,2,14,1,7,5,4,8,15 });
		tree1.Remove(11);
		tree1.Insert(11);
		tree1.Remove(15);
		RBTree<int> tree2;
		tree2.Insert({ 11,2,14,1,7,5,4,8,15 });
		swap(tree1, tree2);
	}
	_CrtDumpMemoryLeaks();
}