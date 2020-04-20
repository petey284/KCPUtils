using System;
using System.Collections.Generic;
using System.Linq;

namespace KCPUtils.CommandLine
{
    // TODO : Provdie support for writing usage instructions

    public interface IArgument { }

    public class Argument : IArgument
    {
        public string Val { get; set; }

        public Argument(string val)
        {
            this.Val = val;
        }
    }

    public class Switch : IArgument
    {
        public string Name { get; set; }
        public bool IsPresent { get; set; }
        public Argument Next { get; set; }

        public Switch(string name) { this.Name = name; }

        public Switch(string name, bool isPresent, Argument next=null)
        {
            this.Name = name;
            if (isPresent)
            {
                this.IsPresent = isPresent;
                this.Next = next ?? default;
            }
        }
    }

    public class ProcessedArgs
    {
        public List<Switch> Switches;
        public List<Argument> Arguments;
    }

    public struct ArgumentBox
    {
        public IEnumerable<string> Arguments;
    }

    public static class CommandLine
    {
        public static void Deconstruct(this ProcessedArgs processed, out List<Switch> switches, out List<Argument> arguments)
        {
            switches = processed.Switches;
            arguments = processed.Arguments;
        }

        public static Switch GetSwitch(this List<Switch> processedSwitches, params string[] search)
        {
            foreach (var item in search)
            {
                var result = processedSwitches.FirstOrDefault(x => x.Name == item);

                if (result != null) { return result; }
            }

            // Return empty switch with the first provided switch name
            return new Switch(search[0]);
        }

        public static string SafeGrab(this string[] arguments, int index)
        {
            return (index < arguments.Count()) ? arguments[index] : string.Empty;
        }

        public static (bool, IEnumerable<string>) Pluck(
            this IEnumerable<string> arguments,
            string searchArg)
        {
            var result = false;
            result = arguments.Any(x => x == searchArg);
            if (result)
            {
                arguments = arguments.Where(x => x != searchArg);
            }

            return (result, arguments);
        }

        public static Dictionary<string, bool> PluckMany(
            this IEnumerable<string> arguments,
            IEnumerable<string> searchArgs)
        {
            var tempStruct = new ArgumentBox()
            {
                Arguments = arguments
            };

            var dict = new Dictionary<string, bool>();
            foreach (var search in searchArgs)
            {
                // CaptureKey draws down the arguments and ensures that
                // we don't go over a value twice.
                var (key, value) = tempStruct.CaptureKey(search);

                // Add key and value to dictionary if not null
                if (key != null) { dict.Add(key, value); }
            }

            return dict;
        }

        public static KeyValuePair<string, bool> CaptureKey(
            ref this ArgumentBox argBox,
            string key)
        {
            var arguments = argBox.Arguments;
            var result = default(KeyValuePair<string, bool>);
            foreach (var elem in arguments)
            {
                if (elem == key)
                {
                    result = new KeyValuePair<string, bool>(elem, true);

                    argBox = new ArgumentBox()
                    {
                        // reduce number of elements if key found
                        Arguments = arguments.Where(x => x != key)
                    };
                    break;
                }
            }

            return result;
        }

        public static ProcessedArgs ProcessArgs(string[] input, List<string> switchOptions)
        {
            // Gather info on switches
            var switchesInfo = input.ToList().PluckMany(switchOptions);

            var result = new ProcessedArgs() { };

            var arguments = new List<Argument>();
            var switches = new List<Switch>();

            // Process arguments sequentially
            for (var index = 0; index < input.Length; index++)
            {
                var pos = index;
                var elem = input[index];
                var isSwitch = switchesInfo.ContainsKey(elem);

                if (isSwitch)
                {
                    var found = switchesInfo[elem];

                    var nextArg = input.SafeGrab(++pos);
                    var next = (found) ? new Argument(nextArg) : default;

                    var _switch = new Switch(elem, found, next);
                    switches.Add(_switch);
                    continue;
                }

                arguments.Add(new Argument(elem));
            }

            result.Switches = switches;
            result.Arguments = arguments;

            return result;
        }
    }
}