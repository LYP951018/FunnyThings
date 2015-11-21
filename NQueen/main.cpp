#define RANGES_SUPPRESS_IOTA_WARNING

#include <range\v3\all.hpp>
#include <vector>
#include <string>
#include <iostream>

using namespace ranges::v3;
using std::uint64_t;

//Naive way to solve N-Queen

template<typename ContainerT>
void Dfs(ContainerT& nowSolution,
	uint64_t row,
	uint64_t n,
	uint64_t& total)
{
	if (row == n)
		++total;
	else
		for_each(view::iota(uint64_t{}, n), [&](auto x) {
			if (all_of(view::iota(uint64_t{}, row), [&](auto y)
			{
				return nowSolution[y][x] != 'y' &&
					(y > x || nowSolution[row - y - 1][x - y - 1] != 'y') &&
					(x + y > n || nowSolution[row - y - 1][x + y + 1] != 'y');
			}))
			{
				nowSolution[row][x] = 'y';
				Dfs(nowSolution, row + 1, n, total);
				nowSolution[row][x] = 0;
			}
		});
}

auto Solve1(uint64_t n)
{
	uint64_t res{};
	std::vector<std::string> nowSolution((uint64_t(n)));
	for_each(nowSolution, [n](auto& str) {str.resize(n + 1);});
	Dfs(nowSolution, 0, n,res);
	return res;
}

//permutation

uint64_t Solve2(uint64_t n)
{
	auto cols = to_vector(view::iota(uint64_t{}, n));
	auto vec = cols;
	uint64_t ans{};
	do 
	{
		if (size(std::vector<uint64_t>(cols) | action::transform([&](auto x) {return vec[x] - x;}) | action::sort | action::unique) == n &&
			size(std::vector<uint64_t>(cols) | action::transform([&](auto x) {return vec[x] + x;}) | action::sort | action::unique) == n) ++ans;
	} while (next_permutation(vec));
	return ans;
}

//bit operations
//http://www.matrix67.com/blog/archives/266


void Dfs2(uint64_t row,uint64_t ld,uint64_t rd,uint64_t upperLim,uint64_t& ans)
{
	uint64_t pos, p;
	if (row != upperLim)
	{
		pos = upperLim & ~(row | ld | rd);
		while (pos != 0)
		{
			p = pos & (-pos);//得到可以放皇后的最右位置
			pos = pos - p;//放上皇后
			Dfs2(row + p, (ld + p) << 1, (rd + p) >> 1,upperLim, ans);
		}
	}
	else ++ans;
}

uint64_t Solve3(uint64_t n)
{
	uint64_t ans{};
	uint64_t upperLim = (uint64_t{ 1 } << n) - 1;
	Dfs2(0, 0, 0, upperLim, ans);
	return ans;
}


int main()
{
	std::cout << Solve1(8) << std::endl;
	std::cout << Solve2(8) << std::endl;
	std::cout << Solve3(8) << std::endl;
	std::system("pause");
}