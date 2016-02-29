#include "Lexer.h"
#include <fstream>

int main()
{
	const funny::StringType str = L"123.4=+-5*4||2";
	TokenInfo info;
	Lexer lexer{ str };
	std::wofstream of{ ".\\result.txt" };
	do
	{
		lexer.GetToken(info);
		of << L"( " << info.Text_ << L" " << static_cast<int>(info.Kind_) << " )" << L"\n";
	} while (info.Kind_ != SyntaxKind::EndOfFileToken);

	std::getchar();
	return 0;
}
