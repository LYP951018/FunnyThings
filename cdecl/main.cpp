#include "Cdecl.h"
#include <iostream>

int main()
{
	
	Cdecl cd{ L"char (*(*x[3])())[5]" };
	cd.Start();
	std::wcout << cd.GetAnswer() << "\n";
	std::system("pause");
	return 0;
}