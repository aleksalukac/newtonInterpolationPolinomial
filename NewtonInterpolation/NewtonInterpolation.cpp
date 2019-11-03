// NewtonInterpolation.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <vector>
#include <cstdlib> 
#include <ctime> 

using namespace std;

struct Point
{
	long double x;
	long double y;

	Point(int a, int b)
	{
		x = a;
		y = b;
	}
};

int n = 12;
vector <Point> points;

long double memory[300][300] = { {0} };
bool filled[300][300] = { {false} };

long double y(int index)
{
	return points[index].y;
}

long double x(int index)
{
	return points[index].x;
}

long double y(int down, int up)
{
	if (down == up)
		return y(down);

	if (filled[down][up])
	{
		return memory[down][up];
	}

	if (up - down == 1)
	{
		memory[down][up] = (points[up].y - points[down].y) / (points[up].x - points[down].x);
		return memory[down][up];
	}

	return (y(down, up - 1) - y(down + 1, up)) / (x(down) - x(up));
}

long double precision(int prec, long double s)
{
	if (prec == 0)
	{
		return y(0);
	}
	long double result = y(0, prec);
	for (int i = 0; i < prec; i++)
	{
		result *= s - x(i);
	}
	return result;
}

long double f(long double s)
{
	long double sum = 0;
	for (int i = 0; i < n; i++)
	{
		long double temp = precision(i, s);
		sum += temp;
		//sum += precision(i, s);
	}

	return sum;
}

void generateRandomPoints()
{
	for (int i = 0; i < n; i++)
	{
		Point a(rand() % 100, rand() % 1000);

		//Proba 
		/*a.x = i;
		a.y = i*i;*/

		points.push_back(a);
	}
}

int main()
{
	srand((unsigned)time(0));
	generateRandomPoints();

	long double x;
	cin >> x;

	cout << "f(x) = " << f(x) << endl;
	return 0;
}
