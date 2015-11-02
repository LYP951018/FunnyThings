#pragma once

#include "SlidingTextWindow.h"
#include <vector>
#include <unordered_map>


/*

  CFG is a quadruple (T,NT,S,P) where

  T: Terminal symbols
  NT: Nonterminal symbols
  S: Goal symbol or start symbol
  P: productions.

  for example:

  NT
  {
	SheepNoise;
  }
  T
  {
	b;a;
  }
  S
  {
	S
  }
  P
  {
	SheepNoise : aab | SheepNoise'
	SheepNoise' : ^e
  }

*/

using namespace std::string_literals;

enum class SymbolType { Start,Terminal,Nonterminal,Epsilon };

struct RuleUnit
{
	SymbolType type_;
	StringType content_;
};

struct Rule
{
	std::vector<RuleUnit> leftHand_;
	std::vector<RuleUnit> rightHand_;
};

class RuleReader
{
public:
	RuleReader(StringType str)
		:textWindow_{ std::move(str) } {}

private:

	void FillIn()
	{
		while (true)
		{
			SkipSpace();
			if (textWindow_.AdvanceIfMatches(TEXT("NT")))
			{

			}
		}
	}

	void SkipSpace()
	{
		auto c = textWindow_.PeekChar();
		while (c != SlidingTextWindow::InvalidCharacter && CharUtility::IsSpace(c))
		{
			textWindow_.AdvanceChar();
		}
	}



	std::unordered_map<StringType, SymbolType> symbolMap_;

	SlidingTextWindow textWindow_;

};