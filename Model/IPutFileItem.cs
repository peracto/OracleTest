﻿namespace Snowflake.FileStream
{
    public interface IPutFileItem
    {
        string Filename { get; }
        string Key { get; }
    }
}