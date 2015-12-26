#include "Lexer.h"

#include <iostream>

int main()
{
	const funny::StringType str = L"123.4+-5*4||2";
	TokenInfo info;
	Lexer lexer{ str };
	do
	{
		lexer.GetToken(info);
		std::cout << (funny::uint32)info.Kind_ << " ";
	} while (info.Kind_ != SyntaxKind::EndOfFileToken);
	std::getchar();

	return 0;
}
