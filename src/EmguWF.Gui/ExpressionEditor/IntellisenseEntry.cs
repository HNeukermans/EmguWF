using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EmguWF.ExpressionEditor
{
    /// <summary>
    /// This represents a single entry in the intellisense tree graph we build from
    /// assemblies and namespaces/types.
    /// </summary>
    [DebuggerDisplay("{Type} {Name} - {SystemType}, Count={Children.Count}  ")]
    public class IntellisenseEntry
    {
        /// <summary>
        /// Parent node (null if this is a namespace or variable root)
        /// </summary>
        public IntellisenseEntry Parent { get; set; }

        /// <summary>
        /// Node type (class, method, field, etc.)
        /// </summary>
        public IntellisenseEntryType Type { get; set; }

        /// <summary>
        /// Type of this element for a property/field
        /// </summary>
        public Type SystemType { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Children
        /// </summary>
        public List<IntellisenseEntry> Children { get; private set; }

        /// <summary>
        /// Name of the node without generic information
        /// </summary>
        public string SimpleName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public IntellisenseEntry()
        {
            Children = new List<IntellisenseEntry>();
            Name = string.Empty;
            SimpleName = string.Empty;
        }

        /// <summary>
        /// Adds a node to the children collection
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(IntellisenseEntry node)
        {
            lock (Children)
            {
                this.Children.Add(node);
            }
        }

        public bool HasNamespace
        {
            get
            {
                return Type == IntellisenseEntryType.Class
                       || Type == IntellisenseEntryType.Enum
                       || Type == IntellisenseEntryType.Interface
                       || Type == IntellisenseEntryType.ValueType;
            }
        }

        /// <summary>
        /// Namespace of the type
        /// </summary>
        public string Namespace
        {
            get
            {
                if (Type == IntellisenseEntryType.Namespace)
                    return FullPath;
                
                var node = Parent;
                while (node != null)
                {
                    if (node.Type == IntellisenseEntryType.Namespace)
                        return node.FullPath;
                    node = node.Parent;
                }
                return null;
            }
        }

        /// <summary>
        /// Retrieve the full path of this node including all parent names.
        /// </summary>
        /// <returns></returns>
        public string FullPath
        {
            get
            {
                string name = Name;
                if (Parent != null)
                {
                    string fullPath = Parent.FullPath;
                    if (!string.IsNullOrWhiteSpace(fullPath))
                    {
                        name = fullPath + "." + name;
                    }
                }
                return name;
            }
        }

        /// <summary>
        /// Locate a node by name
        /// </summary>
        /// <param name="namePath"></param>
        /// <returns></returns>
        public IntellisenseEntry SearchNodes(string namePath)
        {
            return SearchNodesInternal(this, namePath);
        }

        /// <summary>
        /// Locate the best node from a set of paths.
        /// </summary>
        /// <param name="namePaths"></param>
        /// <returns></returns>
        public async Task<IntellisenseEntry> SearchNodesAsync(IEnumerable<string> namePaths)
        {
            var results = await Task.WhenAll(
                namePaths.Select(n => Task.Run(() => SearchNodesInternal(this, n))).ToArray());
            var node = new IntellisenseEntry();
            node.Children.AddRange(results.SelectMany(r => r.RestrictViewToPartialType(namePaths.First())));
            return node;
        }

        /// <summary>
        /// Internal search mechanism
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="namePath"></param>
        /// <returns></returns>
        private IntellisenseEntry SearchNodesInternal(IntellisenseEntry targetNodes, string namePath)
        {
            var targetPath = namePath.Split('.')[0];
            if (string.IsNullOrWhiteSpace(targetPath))
                return targetNodes;

            var validNodes = (targetNodes.Children.Where(str => 
                String.Equals(str.Name, targetPath, StringComparison.InvariantCultureIgnoreCase)));

            IntellisenseEntry locatedNode = validNodes.FirstOrDefault();
            if (locatedNode == null)
                return targetNodes;

            string nextPath = namePath.Substring(targetPath.Length, namePath.Length - targetPath.Length);
            if (nextPath.StartsWith("."))
                nextPath = nextPath.Substring(1, nextPath.Length - 1);
            
            return string.IsNullOrWhiteSpace(nextPath) 
                ? locatedNode 
                : SearchNodesInternal(locatedNode, nextPath);
        }

        public List<IntellisenseEntry> RestrictViewToPartialType(string partialType)
        {
            if (this.Type != IntellisenseEntryType.Namespace)
            {
                return partialType.EndsWith(".") 
                    ? Children 
                    : new List<IntellisenseEntry>();
            }

            bool lookForExactMatch = false;
            var targetText = partialType.ToLowerInvariant();
            if (targetText.EndsWith(")."))
            {
                int pos = targetText.LastIndexOf('(');
                if (pos >= 0)
                    targetText = targetText.Substring(0, pos);
                lookForExactMatch = true;
            }

            targetText = targetText.EndsWith(".") ? targetText.Substring(0, targetText.Length - 1) : targetText.Split('.').LastOrDefault();

            if (string.IsNullOrWhiteSpace(targetText) || targetText == FullPath.ToLowerInvariant())
                return Children;

            var searchList = Children.Where(str => str.Name.ToLowerInvariant().Contains(targetText)).ToList();
            if (IsVariableOrArgument(targetText))
            {
                var itemNode = SearchNodes(targetText);
                if (itemNode != null)
                {
                    var itemNodeType = SearchNodes(itemNode.SystemType.FullName);
                    if (itemNodeType != null && itemNodeType.Children.Count > 0)
                    {
                        if (lookForExactMatch)
                        {
                            searchList.Clear();
                            searchList.AddRange(itemNode.Children);
                        }
                        else
                            searchList.AddRange(itemNodeType.Children);
                    }
                }
            }

            return searchList;
        }

        /// <summary>
        /// Check for a variable
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public bool IsVariableOrArgument(string inputText)
        {
            return Children.Any(s => String.Equals(s.Name, inputText, StringComparison.InvariantCultureIgnoreCase)
                                     && s.SystemType != null);
        }
    }
}