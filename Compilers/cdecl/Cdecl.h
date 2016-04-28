#pragma once


#include <string>
#include <unordered_map>
#include <vector>
#include <exception>



struct InvalidSyntax : std::exception
{
	InvalidSyntax()
		:exception("Invalid syntax!")
	{}

	InvalidSyntax(const char* str)
		:exception(str)
	{}
};

struct InvalidTypeName :InvalidSyntax
{
	InvalidTypeName()
		:InvalidSyntax("invalid type name !")
	{}
};

struct InvalidBrace : InvalidSyntax
{
	InvalidBrace()
		:InvalidSyntax("invalid brace!")
	{}
};
class Cdecl
{
public:
	Cdecl(std::wstring str);
	enum class TokenType
	{
		kTypeQualifier,
		kTypeSpecifier,
		kStorageClassSpecifier,
		kIdentifier,
		kBrackets,
		kStar,
		kLeftBrace,
		kRightBrace,
		kComma
	};
	void Start(bool isAbstract = false);
	std::wstring GetAnswer(bool isAbstract);
private:
	
	
	void SkipSpace();
	bool GetToken();
	void UnGetToken();
	TokenType GetType(const std::wstring& token);
	std::wstring GetTypeName();
	bool ParsePointer(std::wstring& str);
	void ParseDeclarator(bool isAbstract);
	void ParseDirectDeclarator(bool isAbstract);

	static const std::wstring TypeQualifierName[3];
	static const std::wstring TypeSpecifierName[14];
	static std::unordered_map<std::wstring, TokenType> symbolTable;

	
	const std::wstring stringToParse_;
	std::size_t curPos_{};
	std::wstring answer_;
	std::wstring dataName_;
	std::vector<std::wstring> dataType_;
	std::wstring curToken_;
	TokenType curTokenType_{};
};