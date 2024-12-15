using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NeeLaboratory.IO
{
    /// <summary>
    /// ファイルシステム規約に依存しないパス文字列ユーティリティ
    /// ファイル名に使用できない文字を含んだパスの解析用
    /// </summary>
    public static class LoosePath
    {
        public static readonly char[] Separator = new char[] { '\\', '/' };
        public static readonly char DefaultSeparator = '\\';

        public static readonly char[] AsciiSpaces = new char[] {
            '\u0009',  // CHARACTER TABULATION
            '\u000A',  // LINE FEED
            '\u000B',  // LINE TABULATION
            '\u000C',  // FORM FEED
            '\u000D',  // CARRIAGE RETURN
            '\u0020',  // SPACE
        };

        /// <summary>
        /// 末尾のセパレート記号を削除。
        /// ルート(C:\)の場合は削除しない
        /// </summary>
        public static string TrimEnd(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            if (Separator.Contains(s.Last()))
            {
                s = s.TrimEnd(Separator);
                if (s.Last() == ':') s += "\\";
            }

            return s;
        }

        /// <summary>
        /// ディレクトリ名用に、終端にセパレート記号を付加する
        /// </summary>
        public static string TrimDirectoryEnd(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            return s.TrimEnd(Separator) + '\\';
        }

        public static string[] Split(string s)
        {
            if (string.IsNullOrEmpty(s)) return Array.Empty<string>();

            return s.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            s = NormalizeSeparator(s);
            var index = s.Length;
            while (index > 0 && s[index - 1] == DefaultSeparator)
            {
                index--;
            }
            var end = index;
            while (index > 0 && s[index - 1] != DefaultSeparator)
            {
                index--;
            }
            var head = index;
            while (head > 0 && s[head - 1] == DefaultSeparator)
            {
                head--;
            }

            return head == 0 ? s[..end] : s[index..end];
        }

        // place部をディレクトリーとみなしたファイル名取得
        public static string GetFileName(string s, string place)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (string.IsNullOrEmpty(place)) return s;
            if (string.Compare(s, 0, place, 0, place.Length, StringComparison.Ordinal) != 0) throw new ArgumentException("s not contain place");

            return s.Substring(place.Length).TrimStart(Separator);
        }

        public static string GetPathRoot(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var parts = s.Split(Separator, 2);
            return parts.First();
        }

        public static string GetDirectoryName(string s, bool fixRootDirectory = true)
        {
            if (string.IsNullOrEmpty(s)) return "";

            s = NormalizeSeparator(s);
            var index = s.Length;
            while (index > 0 && s[index - 1] == DefaultSeparator)
            {
                index--;
            }
            while (index > 0 && s[index - 1] != DefaultSeparator)
            {
                index--;
            }
            while (index > 0 && s[index - 1] == DefaultSeparator)
            {
                index--;
            }

            if (index < 0) return "";

            var answer = s[0..index];
            if (fixRootDirectory && answer.Length > 0 && answer.Last() == ':') answer += DefaultSeparator;
            return answer;
        }

        public static string GetExtension(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            string fileName = GetFileName(s);
            int index = fileName.LastIndexOf('.');

            return (index >= 0) ? fileName.Substring(index).ToLower(CultureInfo.CurrentCulture) : "";
        }

        public static string Combine(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return s2;
            else if (string.IsNullOrEmpty(s2))
                return s1;
            else
                return s1.TrimEnd(Separator) + "\\" + s2.TrimStart(Separator);
        }

        // ファイル名として使えない文字を置換
        public static string ValidFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            string valid = s;
            char[] invalidch = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in invalidch)
            {
                valid = valid.Replace(c, '_');
            }
            return valid;
        }

        // セパレータ標準化
        public static string NormalizeSeparator(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            return s.Replace('/', '\\');
        }

        // UNC判定
        public static bool IsUnc(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            var head = GetHeadSeparators(s);
            return head.Length == 2;
        }

        // パス先頭にあるセパレータ部を取得
        private static string GetHeadSeparators(string s)
        {
            var slashCount = 0;
            foreach (var c in s)
            {
                if (c == '\\' || c == '/')
                {
                    slashCount++;
                }
                else
                {
                    break;
                }
            }

            return slashCount > 0 ? new string('\\', slashCount) : "";
        }

        // 表示用のファイル名生成
        public static string GetDispName(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "PC";
            }
            else
            {
                // ドライブ名なら終端に「￥」を付ける
                var name = LoosePath.GetFileName(s);
                if (s.Length <= 3 && name.Length == 2 && name[1] == ':')
                {
                    name += '\\';
                }
                return name;
            }
        }

        /// <summary>
        /// パスを "FooBar (C:\Parent)" 形式にする
        /// </summary>
        public static string GetPlaceName(string s)
        {
            var name = GetFileName(s);
            var parent = GetDirectoryName(s);

            if (string.IsNullOrEmpty(parent))
            {
                return name;
            }
            else
            {
                return name + " (" + parent + ")";
            }
        }
    }
}
