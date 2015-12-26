#pragma once

#include "SlidingTextWindow.h"
#include "AbstractLexer.h"
#include "SyntaxKind.h"
#include "TokenInfo.h"


class Lexer : public funny::AbstractLexer
{
public:
	void GetToken(TokenInfo& info);

	Lexer(const funny::StringType& str)
		:AbstractLexer{str}
	{}

private:
	bool ScanNumericLiteral(TokenInfo& info);

	void ScanNumericLiteralSingleInteger(funny::StringType& builder);
};
