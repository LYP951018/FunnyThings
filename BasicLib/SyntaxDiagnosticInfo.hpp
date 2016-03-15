#pragma once

#include "StringType.hpp"
#include "Basic.hpp"
#include <unordered_map>
#include <boost\optional.hpp>

namespace funny
{
	class SyntaxDiagnosticInfo
	{
	public:
		using size_type = std::size_t;
		using DescriptorMapType = std::unordered_map<uint32, StringType>;

		//TODO: exception spec & other special member functions

		SyntaxDiagnosticInfo(size_type offset,/*size_type width,*/uint32 errorCode)
			:offset_{ offset },
			//width_{width},
			errorCode_{ errorCode }
		{}

		SyntaxDiagnosticInfo(uint32 errorCode)
			:SyntaxDiagnosticInfo{ {},errorCode }
		{}

		boost::optional<StringType> GetDescriptor(const DescriptorMapType& descripters);

	private:
		size_type offset_;
		//size_type width_;
		uint32 errorCode_;
	};

}