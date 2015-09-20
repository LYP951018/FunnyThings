#include "AvlTree.h"


int main()
{
	{
		AvlTree<int> tree;
		tree.Insert({ 3,2,1,4,7,4,5,6,10,16,8,9,15,14,13,11 });
		tree.Remove(7);
		int i = 0;
	}
	_CrtDumpMemoryLeaks();
}