#include "Lexer.h"

void Lexer::GetToken(TokenInfo & info)
{
	info = {};
	funny::CharType ch = textWindow_.PeekChar();

	switch (ch)
	{
	case L'+':
		textWindow_.AdvanceChar();
		info.Kind_ = SyntaxKind::PlusToken;
		info.Text_ = L"+";
		break;
	case L'-':
		textWindow_.AdvanceChar();
		info.Kind_ = SyntaxKind::MinusToken;
		info.Text_ = L"-";
		break;
	case L'/':
		textWindow_.AdvanceChar();
		info.Kind_ = SyntaxKind::SlashToken;
		info.Text_ = L"/";
		break;
	case L'*':
		textWindow_.AdvanceChar();
		info.Kind_ = SyntaxKind::AsteriskToken;
		info.Text_ = L"*";
		break;
	case L'=':
		textWindow_.AdvanceChar();
		if (textWindow_.PeekChar() == L'=')
		{
			textWindow_.AdvanceChar();
			info.Kind_ = SyntaxKind::EqualsEqualsToken;
			info.Text_ = L"==";
		}
		else
		{
			info.Kind_ = SyntaxKind::EqualsToken;
			info.Text_ = L"=";
		}
		break;
	case L'|':
		textWindow_.AdvanceChar();
		if (textWindow_.PeekChar() == L'|')
		{
			textWindow_.AdvanceChar();
			info.Kind_ = SyntaxKind::BarBarToken;
			info.Text_ = L"||";
		}
		else
		{
			info.Kind_ = SyntaxKind::BarToken;
			info.Text_ = L"|";
		}			
		break;
	case L'&':
		textWindow_.AdvanceChar();
		if (textWindow_.PeekChar() == L'&')
		{
			textWindow_.AdvanceChar();
			info.Kind_ = SyntaxKind::AmpersandAmpersandToken;
		}
		else
			info.Kind_ = SyntaxKind::AmpersandToken;
		break;
	case L'1':
	case L'2':
	case L'3':
	case L'4':
	case L'5':
	case L'6':
	case L'7':
	case L'8':
	case L'9':
	case L'0':
		ScanNumericLiteral(info);
		break;
	case funny::SlidingTextWindow::InvalidCharacter:
		info.Kind_ = SyntaxKind::EndOfFileToken;
		break;
	default:
		break;
	}
}

bool Lexer::ScanNumericLiteral(TokenInfo & info)
{
	//TODO: add support for hex & binary
	funny::StringType builder;
	ScanNumericLiteralSingleInteger(builder);
	auto ch = textWindow_.PeekChar();
	bool isDecimal = {};
	if (ch == L'.')
	{
		auto ch2 = textWindow_.PeekChar(1);
		if (funny::CharUtility::IsDigit(ch2))
		{
			isDecimal = true;
			builder.push_back(ch);
			textWindow_.AdvanceChar();
			ScanNumericLiteralSingleInteger(builder);
			info.Text_ = builder;
		}
		//TODO: Process dot.
	}
	else
		info.Text_ = builder;

	//TODO: complete all kinds of literals
	if ((ch = textWindow_.PeekChar()) == L'E' || ch == L'e')
	{

	}
	info.Kind_ = SyntaxKind::NumericLiteralToken;
	//TODO: replace stoi & stod with custom functions.
	if (isDecimal)
	{		
		info.Type_ = PrimitiveType::Double;
		info.DoubleValue_ = std::stod(builder);
	}	
	else
	{
		info.Type_ = PrimitiveType::Int32;
		info.Int32Value_ = std::stoi(builder);
	}
	//TODO: Handle failure cases.
	return true;
}

void Lexer::ScanNumericLiteralSingleInteger(funny::StringType & builder)
{
	while (true)
	{
		auto ch = textWindow_.PeekChar();
		if (!funny::CharUtility::IsDigit(ch))
			break;
		builder.push_back(ch);
		textWindow_.AdvanceChar();
	}
}
