#pragma once


#include <string>
#include <unordered_map>
#include <vector>
#include <exception>

struct InvalidTypeName :std::exception
{
	InvalidTypeName()
		:exception("invalid type name !")
	{}
};

struct InvalidBrace : std::exception
{
	InvalidBrace()
		:exception("invalid brace!")
	{}
};

struct InvalidSyntax : std::exception
{
	InvalidSyntax()
		:exception("Invalid syntax!")
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
	
	/*(6.7.5) declarator : 
			pointeropt direct - declarator
		
		
	
		(6.7.5) parameter - type - list :
			parameter - list
			parameter - list, ...*/
	static const std::wstring TypeQualifierName[3];
	static const std::wstring TypeSpecifierName[14];
	static std::unordered_map<std::wstring, TokenType> symbolTable;

	
	const std::wstring stringToParse_;
	std::size_t curPos_{};
	std::wstring answer_;
	std::wstring dataName_;
	std::vector<std::wstring> dataType_;
	std::wstring curToken_;
	std::size_t nowDataTypeIndex_{};
	TokenType curTokenType_{};
};