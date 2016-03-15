#pragma once

namespace funny
{

	struct Noncopyable
	{
		constexpr Noncopyable() noexcept = default;
		Noncopyable(const Noncopyable&) noexcept = delete;
		Noncopyable& operator=(const Noncopyable&) noexcept = delete;
	};


	struct Nonmoveable
	{
		constexpr Nonmoveable() noexcept = default;
		Nonmoveable(Nonmoveable&&) noexcept = delete;
		Nonmoveable& operator=(Nonmoveable&&) noexcept = delete;
	};

	struct Noncopymoveable
	{
		constexpr Noncopymoveable() noexcept = default;
		Noncopymoveable(const Noncopymoveable&) noexcept = delete;
		Noncopymoveable& operator=(const Noncopymoveable&) noexcept = delete;
		Noncopymoveable(Noncopymoveable&&) noexcept = delete;
		Noncopymoveable& operator=(Noncopymoveable&&) noexcept = delete;
	};
}

