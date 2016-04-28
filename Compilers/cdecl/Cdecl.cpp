#include "Cdecl.h"

#include <iostream>
#include <cwctype>
#include <sstream>

using namespace std::literals;

const std::wstring Cdecl::TypeQualifierName[3] =
{
	L"const",
	L"restrict",
	L"volatile"
};

const std::wstring Cdecl::TypeSpecifierName[14] =
{
	L"void",
	L"char",
	L"short",
	L"int",
	L"long",
	L"float",
	L"double",
	L"signed",
	L"unsigned",
	L"_Bool",
	L"_Complex",
	L"struct",
	L"enum",
	L"typedef's name(not support)"
};

std::unordered_map<std::wstring, Cdecl::TokenType> Cdecl::symbolTable{};

Cdecl::Cdecl(std::wstring str)
	:stringToParse_(std::move(str))
{
	if (symbolTable.empty())
	{
		for (const auto& st : TypeQualifierName)
			symbolTable.insert(std::make_pair(st, TokenType::kTypeQualifier));
		for (const auto& st : TypeSpecifierName)
			symbolTable.insert(std::make_pair(st, TokenType::kTypeSpecifier));
	}
}

void Cdecl::Start(bool isAbstract)
{
	dataType_.push_back(GetTypeName());
	ParseDeclarator(isAbstract);
}

std::wstring Cdecl::GetAnswer(bool isAbstract)
{
	auto ret = (isAbstract ? L""s : (dataName_ + L" is"s))+ answer_ + L" "s + dataType_.back();
	if (dataType_.size() > 1)
	{
		dataType_.pop_back();
	}
	return ret;
}

void Cdecl::SkipSpace()
{
	while (std::iswspace(stringToParse_[curPos_]))
	{
		++curPos_;
	}
}

bool Cdecl::GetToken()
{
	SkipSpace();
	if (curPos_ >= stringToParse_.size()) return false;
	auto startPos = curPos_;
	wchar_t c = stringToParse_[curPos_++];
	switch (c)
	{
	case L'*':
		curTokenType_ = TokenType::kStar;
		break;
	case L'(':
		curTokenType_ = TokenType::kLeftBrace;
		break;
	case L')':
		curTokenType_ = TokenType::kRightBrace;
		break;
	case L',':
		curTokenType_ = TokenType::kComma;
		break;
	case L'[':
	{
		
			for (;curPos_ != stringToParse_.size() && stringToParse_[curPos_] != L']';++curPos_)
				;
			if (curPos_ == stringToParse_.size())
				throw InvalidBrace();
			++curPos_;
			curTokenType_ = TokenType::kBrackets;
		
		break;
	}
	default:
	{
		for (;;++curPos_)
		{
			c = stringToParse_[curPos_];
			if (!(std::iswalpha(c) || std::iswdigit(c))) break;
		}
		curToken_ = stringToParse_.substr(startPos, curPos_ - startPos);
		curTokenType_ = GetType(curToken_);
		return true;
	}
		break;
	}
	
	curToken_ = stringToParse_.substr(startPos, curPos_ - startPos);
	return true;
}

void Cdecl::UnGetToken()
{
	curPos_ -= curToken_.size();
}


Cdecl::TokenType Cdecl::GetType(const std::wstring & token)
{
	auto it = symbolTable.find(token);
	if (it != symbolTable.cend())
	{
		return it->second;
	}
	return TokenType::kIdentifier;
}

std::wstring Cdecl::GetTypeName()
{
	std::wstring typeName ;
	GetToken();
	bool hasSigned = false;
	if (curTokenType_ == TokenType::kTypeQualifier)
	{
		typeName += curToken_;
	}
	else UnGetToken();

	for (std::size_t i{};GetToken()&&curTokenType_ == TokenType::kTypeSpecifier;++i)
	{
		if (curToken_ == L"signed" || curToken_ == L"unsigned")
			if (i == 0)
				hasSigned = true;
			else throw InvalidTypeName();
		if (i == 1 && (!hasSigned ||
			(
				curToken_ != L"char" &&
				curToken_ != L"short" &&
				curToken_ != L"int" &&
				curToken_ != L"long")))
		{
			throw InvalidTypeName();
		}
		if(i == 2 && (!hasSigned || curToken_ != L"long")) throw InvalidTypeName();
		if (i > 2) throw InvalidTypeName();
		typeName += L" "s += curToken_;
	}
	UnGetToken();
	return std::move(typeName);
}

bool Cdecl::ParsePointer(std::wstring& str)
{
	/*(6.7.5) pointer: * type - quali?er - listopt 
					   * type - quali?er - listopt pointer*/
	if (!GetToken()) return false;
	std::wstring pointerQualifier;
	if (curTokenType_ == TokenType::kStar)
	{
		//This is a pointer
		while (true)
		{
			if (!GetToken()) break;
			if (curTokenType_ == TokenType::kTypeQualifier) pointerQualifier += L" "s += curToken_;
			else
			{
				if(curPos_ != stringToParse_.size())
					UnGetToken();
				break;
			}
		}
		str += L" "s += pointerQualifier += (pointerQualifier.empty() ? L""s : L" "s)+= L"pointer point to";
		ParsePointer(str);
		return true;
	};
	UnGetToken();
	return false;
}

void Cdecl::ParseDeclarator(bool isAbstract)
{
	std::wstring str;
	if (isAbstract)
	{
		ParsePointer(str);
		auto tempCurPos = curPos_;
		try
		{
			ParseDirectDeclarator(isAbstract);
		}
		catch (const std::exception&)
		{
			curPos_ = tempCurPos;
		}
		
	}
	else
	{
		ParsePointer(str);
		ParseDirectDeclarator(isAbstract);
	}
	answer_ += str;
}

void Cdecl::ParseDirectDeclarator(bool isAbstract)
{
	GetToken();
	if (curTokenType_ == TokenType::kLeftBrace)
	{
		ParseDeclarator(isAbstract);
		GetToken();
		if (curTokenType_ != TokenType::kRightBrace) throw InvalidBrace();
	}
	else if (curTokenType_ == TokenType::kIdentifier)
		dataName_ = curToken_;
	else throw InvalidSyntax();
	start:
	{
		if (!GetToken()) 
			return;
		if (curTokenType_ == TokenType::kBrackets)
		{
			answer_ += L" an array "s += curToken_ += L" of";
			goto start;
		}
		else if (curTokenType_ == TokenType::kLeftBrace)
		{
			answer_ += L" \nfunction with params: \n";
			GetToken();
			if (curTokenType_ == TokenType::kRightBrace)
			{
				answer_ += L"void"s += L" returning";
				goto start;
			}
			UnGetToken();
			while (true)
			{
				Start(true);
				answer_ = GetAnswer(true);
				answer_ += L",";
				if (!GetToken()) return;
				if (curTokenType_ == TokenType::kRightBrace) break;
				else if (curTokenType_ != TokenType::kComma) throw InvalidSyntax();
			}
			answer_ += L" returning ";
			goto start;
		}
		else UnGetToken();
	}
}


