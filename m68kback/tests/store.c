void store16(unsigned short* source, unsigned short* dest, int len)
{
	while (len--)
	{
		*dest++ = *source++;
	}
}
