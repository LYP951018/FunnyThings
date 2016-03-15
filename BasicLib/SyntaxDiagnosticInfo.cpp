#include "SyntaxDiagnosticInfo.hpp"

#include <sstream>

boost::optional<funny::StringType> funny::SyntaxDiagnosticInfo::GetDescriptor(const DescriptorMapType & descripters)
{
	auto it = descripters.find(errorCode_);
	if (it != descripters.end())
	{
		funny::StringType ret;
		funny::StringStreamType ss{ ret };
		ss << it->second << TEXT(" ") << "occured at offset" << offset_;
		return ret;
	}
	return{};
}
