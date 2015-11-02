#include "ReadFile.h"
#include <fstream>


StringType ReadFile(const StringType& path)
{
	FileInStreamType input{ path };
	if (input)
	{
		StringType contents;
		input.seekg(0, std::ios::end);
		contents.resize(input.tellg());
		input.seekg(0, std::ios::beg);
		input.read(&contents[0], contents.size());
		return std::move(contents);
	}
}