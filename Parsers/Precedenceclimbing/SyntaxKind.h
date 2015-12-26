#pragma once

enum class SyntaxKind
{
	None,
	CaretToken,//^
	AmpersandToken,//&
	BarToken,//|
	EqualsToken,//=
	PlusToken,//+
	MinusToken,//-
	AsteriskToken,//*
	SlashToken,// /
	OpenParenToken,// (
	CloseParenToken,// )

	BarBarToken, //||
	MinusMinusToken,//--
	PlusPlusToken,//++
	EqualsEqualsToken,//==
	AmpersandAmpersandToken,//&&

	NumericLiteralToken,

	EndOfFileToken
	
};

enum class PrimitiveType
{
	Int32,
	UInt32,
	Float,
	Double
	//...
};

enum class AssociativeDirection
{
	Left,
	Right
};

//TODO: A better name
enum class Rank
{
	Unary,
	Binary
};
