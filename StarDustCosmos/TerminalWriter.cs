using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDustCosmos
{
    public class TerminalWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.Default;
        internal Action<string> OnWrite;

        internal TerminalWriter(Action<string> OnWrite)
        {
            this.OnWrite = OnWrite;
        }

        public override void WriteLine(string? value)
        {
            OnWrite.Invoke(value + "\n");
        }
        public override void Write(string? value)
        {
            OnWrite.Invoke(value);
        }

        // Writes a character to the text stream. This default method is empty,
        // but descendant classes can override the method to provide the
        // appropriate functionality.
        //
        public override void Write(char value)
        {
            OnWrite.Invoke(value.ToString());
        }

        // Writes a character array to the text stream. This default method calls
        // Write(char) for each of the characters in the character array.
        // If the character array is null, nothing is written.
        //
        public override void Write(char[]? buffer)
        {
            if (buffer != null)
            {
                Write(buffer, 0, buffer.Length);
            }
        }

        // Writes a range of a character array to the text stream. This method will
        // write count characters of data into this TextWriter from the
        // buffer character array starting at position index.
        //
        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < count; i++) Write(buffer[index + i]);
        }

        // Writes a span of characters to the text stream.
        //
        public override void Write(ReadOnlySpan<char> buffer)
        {
            char[] array = ArrayPool<char>.Shared.Rent(buffer.Length);

            try
            {
                buffer.CopyTo(new Span<char>(array));
                Write(array, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(array);
            }
        }

        // Writes the text representation of a boolean to the text stream. This
        // method outputs either bool.TrueString or bool.FalseString.
        //
        public override void Write(bool value)
        {
            Write(value ? "True" : "False");
        }

        // Writes the text representation of an integer to the text stream. The
        // text representation of the given value is produced by calling the
        // int.ToString() method.
        //
        public override void Write(int value)
        {
            Write(value.ToString(FormatProvider));
        }

        // Writes the text representation of an integer to the text stream. The
        // text representation of the given value is produced by calling the
        // uint.ToString() method.
        //
        [CLSCompliant(false)]
        public override void Write(uint value)
        {
            Write(value.ToString(FormatProvider));
        }

        // Writes the text representation of a long to the text stream. The
        // text representation of the given value is produced by calling the
        // long.ToString() method.
        //
        public override void Write(long value)
        {
            Write(value.ToString(FormatProvider));
        }

        // Writes the text representation of an unsigned long to the text
        // stream. The text representation of the given value is produced
        // by calling the ulong.ToString() method.
        //
        [CLSCompliant(false)]
        public override void Write(ulong value)
        {
            Write(value.ToString(FormatProvider));
        }

        // Writes the text representation of a float to the text stream. The
        // text representation of the given value is produced by calling the
        // float.ToString(float) method.
        //
        public override void Write(float value)
        {
            Write(value.ToString(FormatProvider));
        }

        // Writes the text representation of a double to the text stream. The
        // text representation of the given value is produced by calling the
        // double.ToString(double) method.
        //
        public override void Write(double value)
        {
            Write(value.ToString(FormatProvider));
        }

        public override void Write(decimal value)
        {
            Write(value.ToString(FormatProvider));
        }

        // Writes the text representation of an object to the text stream. If the
        // given object is null, nothing is written to the text stream.
        // Otherwise, the object's ToString method is called to produce the
        // string representation, and the resulting string is then written to the
        // output stream.
        //
        public override void Write(object? value)
        {
            if (value != null)
            {
                if (value is IFormattable f)
                {
                    Write(f.ToString(null, FormatProvider));
                }
                else
                    Write(value.ToString());
            }
        }

        /// <summary>
        /// Equivalent to Write(stringBuilder.ToString()) however it uses the
        /// StringBuilder.GetChunks() method to avoid creating the intermediate string
        /// </summary>
        /// <param name="value">The string (as a StringBuilder) to write to the stream</param>
        public override void Write(StringBuilder? value)
        {
            if (value != null)
            {
                foreach (ReadOnlyMemory<char> chunk in value.GetChunks())
                    Write(chunk.Span);
            }
        }

        // Writes out a formatted string.  Uses the same semantics as
        // string.Format.
        //
        public override void Write(string format, object? arg0)
        {
            Write(string.Format(FormatProvider, format, arg0));
        }

        // Writes out a formatted string.  Uses the same semantics as
        // string.Format.
        //
        public override void Write(string format, object? arg0, object? arg1)
        {
            Write(string.Format(FormatProvider, format, arg0, arg1));
        }

        // Writes out a formatted string.  Uses the same semantics as
        // string.Format.
        //
        public override void Write(string format, object? arg0, object? arg1, object? arg2)
        {
            Write(string.Format(FormatProvider, format, arg0, arg1, arg2));
        }

        // Writes out a formatted string.  Uses the same semantics as
        // string.Format.
        //
        public override void Write(string format, params object?[] arg)
        {
            Write(string.Format(FormatProvider, format, arg));
        }

        // Writes a line terminator to the text stream. The default line terminator
        // is Environment.NewLine, but this value can be changed by setting the NewLine property.
        //
        public override void WriteLine()
        {
            Write(CoreNewLine);
        }

        // Writes a character followed by a line terminator to the text stream.
        //
        public override void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }

        // Writes an array of characters followed by a line terminator to the text
        // stream.
        //
        public override void WriteLine(char[]? buffer)
        {
            Write(buffer);
            WriteLine();
        }

        // Writes a range of a character array followed by a line terminator to the
        // text stream.
        //
        public override void WriteLine(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            WriteLine();
        }

        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            char[] array = ArrayPool<char>.Shared.Rent(buffer.Length);

            try
            {
                buffer.CopyTo(new Span<char>(array));
                WriteLine(array, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(array);
            }
        }

        // Writes the text representation of a boolean followed by a line
        // terminator to the text stream.
        //
        public override void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }

        // Writes the text representation of an integer followed by a line
        // terminator to the text stream.
        //
        public override void WriteLine(int value)
        {
            Write(value);
            WriteLine();
        }

        // Writes the text representation of an unsigned integer followed by
        // a line terminator to the text stream.
        //
        [CLSCompliant(false)]
        public override void WriteLine(uint value)
        {
            Write(value);
            WriteLine();
        }

        // Writes the text representation of a long followed by a line terminator
        // to the text stream.
        //
        public override void WriteLine(long value)
        {
            Write(value);
            WriteLine();
        }

        // Writes the text representation of an unsigned long followed by
        // a line terminator to the text stream.
        //
        [CLSCompliant(false)]
        public override void WriteLine(ulong value)
        {
            Write(value);
            WriteLine();
        }

        // Writes the text representation of a float followed by a line terminator
        // to the text stream.
        //
        public override void WriteLine(float value)
        {
            Write(value);
            WriteLine();
        }

        // Writes the text representation of a double followed by a line terminator
        // to the text stream.
        //
        public override void WriteLine(double value)
        {
            Write(value);
            WriteLine();
        }

        public override void WriteLine(decimal value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Equivalent to WriteLine(stringBuilder.ToString()) however it uses the
        /// StringBuilder.GetChunks() method to avoid creating the intermediate string
        /// </summary>
        public override void WriteLine(StringBuilder? value)
        {
            Write(value);
            WriteLine();
        }

        // Writes the text representation of an object followed by a line
        // terminator to the text stream.
        //
        public override void WriteLine(object? value)
        {
            if (value == null)
            {
                WriteLine();
            }
            else
            {
                // Call WriteLine(value.ToString), not Write(Object), WriteLine().
                // This makes calls to WriteLine(Object) atomic.
                if (value is IFormattable f)
                {
                    WriteLine(f.ToString(null, FormatProvider));
                }
                else
                {
                    WriteLine(value.ToString());
                }
            }
        }

        // Writes out a formatted string and a new line.  Uses the same
        // semantics as string.Format.
        //
        public override void WriteLine(string format, object? arg0)
        {
            WriteLine(string.Format(FormatProvider, format, arg0));
        }

        // Writes out a formatted string and a new line.  Uses the same
        // semantics as string.Format.
        //
        public override void WriteLine(string format, object? arg0, object? arg1)
        {
            WriteLine(string.Format(FormatProvider, format, arg0, arg1));
        }

        // Writes out a formatted string and a new line.  Uses the same
        // semantics as string.Format.
        //
        public override void WriteLine(string format, object? arg0, object? arg1, object? arg2)
        {
            WriteLine(string.Format(FormatProvider, format, arg0, arg1, arg2));
        }

        // Writes out a formatted string and a new line.  Uses the same
        // semantics as string.Format.
        //
        public override void WriteLine(string format, params object?[] arg)
        {
            WriteLine(string.Format(FormatProvider, format, arg));
        }
    }
}
