#include "Cdecl.h"
#include <iostream>

int main()
{
	std::wstring str;
	while (std::wcin)
	{
		std::getline(std::wcin, str);
		Cdecl cd{ str };
		cd.Start();
		std::wcout << cd.GetAnswer(false) << L"\n";
	}
	Cdecl cd{ L"char (*(*x[3])())[5]" };
	cd.Start();
	std::wcout << cd.GetAnswer(false) << "\n";
	std::system("pause");
	return 0;
}