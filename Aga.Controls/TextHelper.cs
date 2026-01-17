using System.Windows.Forms;
using System.Drawing;

namespace Aga.Controls
{
	public static class TextHelper
	{
		public static StringAlignment TranslateAligment(HorizontalAlignment alignment)
        {
            if (alignment == HorizontalAlignment.Left)
				return StringAlignment.Near;
            if (alignment == HorizontalAlignment.Right)
                return StringAlignment.Far;
            return StringAlignment.Center;
        }

        public static TextFormatFlags TranslateAligmentToFlag(HorizontalAlignment alignment)
        {
            if (alignment == HorizontalAlignment.Left)
                return TextFormatFlags.Left;
            if (alignment == HorizontalAlignment.Right)
                return TextFormatFlags.Right;
            return TextFormatFlags.HorizontalCenter;
        }

		public static TextFormatFlags TranslateTrimmingToFlag(StringTrimming trimming)
		{
			if (trimming == StringTrimming.EllipsisCharacter)
				return TextFormatFlags.EndEllipsis;
            if (trimming == StringTrimming.EllipsisPath)
                return TextFormatFlags.PathEllipsis;
            if (trimming == StringTrimming.EllipsisWord)
				return TextFormatFlags.WordEllipsis;
			if (trimming == StringTrimming.Word)
				return TextFormatFlags.WordBreak;
            return TextFormatFlags.Default;
        }
	}
}
