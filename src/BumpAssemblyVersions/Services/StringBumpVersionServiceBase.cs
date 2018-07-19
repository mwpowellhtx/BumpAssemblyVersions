//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text.RegularExpressions;

//namespace Bav
//{
//    using static String;
//    using static RegexOptions;

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <inheritdoc cref="BumpVersionServiceBase" />
//    public abstract class StringBumpVersionServiceBase<
//            [ConstrainGenericTypes(typeof(AssemblyVersionAttribute)
//                , typeof(AssemblyFileVersionAttribute)
//                , typeof(AssemblyInformationalVersionAttribute))]
//            T>
//        : BumpVersionServiceBase, IStringBumpVersionService
//        where T : Attribute
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="versionProviders"></param>
//        /// <inheritdoc />
//        protected StringBumpVersionServiceBase(params IVersionProvider[] versionProviders)
//            : base(versionProviders)
//        {
//        }

//        //// TODO: TBD: yes, I think this must happen independent of anything having knowledge of any Streams
//        ///// <summary>
//        ///// Returns each of the lines from the <paramref name="stream"/> sans any Carriage
//        ///// Returns or Lines Feeds.
//        ///// </summary>
//        ///// <param name="stream"></param>
//        ///// <returns></returns>
//        //protected static IEnumerable<string> ReadLinesFromStream(Stream stream)
//        //{
//        //    using (var sr = new StreamReader(stream))
//        //    {
//        //        const string cr = @"\r";
//        //        const string lf = @"\n";

//        //        while (!sr.EndOfStream)
//        //        {
//        //            var line = sr.ReadLine() ?? Empty;
//        //            yield return line.Replace(cr, Empty).Replace(lf, Empty);
//        //        }
//        //    }
//        //}

//        ///// <summary>
//        ///// Writes the <paramref name="lines"/> to the <paramref name="stream"/>.
//        ///// </summary>
//        ///// <param name="stream"></param>
//        ///// <param name="lines"></param>
//        //protected static void WriteLinesToStream(Stream stream, IEnumerable<string> lines)
//        //{
//        //    using (var sw = new StreamWriter(stream))
//        //    {
//        //        foreach (var line in lines.ToArray())
//        //        {
//        //            sw.WriteLineAsync(line).Wait();
//        //        }
//        //    }
//        //}

//        /// <inheritdoc />
//        public bool TryBumpVersion(IEnumerable<string> givenLines, out IEnumerable<string> resultLines)
//        {
//            resultLines = givenLines.ToArray();
//            // TODO: TBD: examine the lines, may inject the Reflection using statement, bump the version, etc...
//            return false;
//        }
//    }
//}
