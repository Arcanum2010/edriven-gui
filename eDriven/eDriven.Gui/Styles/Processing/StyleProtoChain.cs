using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using eDriven.Gui.Components;
using UnityEngine;
using Component=eDriven.Gui.Components.Component;

namespace eDriven.Gui.Styles
{
	internal static class StyleProtoChain
	{
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
		internal static readonly StyleTable STYLE_UNINITIALIZED = new StyleTable();

// ReSharper restore InconsistentNaming
// ReSharper restore UnusedMember.Global

#if DEBUG
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
		public static Type TYPE_TO_MONITOR; // = typeof(ImageButtonSkin);
	
// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore FieldCanBeMadeReadOnly.Global
// ReSharper restore UnassignedField.Global
// ReSharper restore InconsistentNaming
// ReSharper restore UnusedMember.Global
#endif

		/**
		 *  
		 *  Implements the getClassStyleDeclarations() logic
		 *  for Component and TextBase.
		 *  The 'object' parameter will be one or the other.
		 */
		internal static List<StyleDeclaration> GetClassStyleDeclarations(IStyleClient client)
		{
			/*if (client.GetType().FullName == "Assets.eDriven.Skins.ImageButtonSkin")
				Debug.Log("GetMatchingStyleDeclarations for: " + client);*/

			StyleManager styleManager = StyleManager.Instance;
			//bool qualified = true;
			string className = client.GetType().FullName;
			//IStyleClient advancedObject = client as IStyleClient;
			OrderedObject<bool> typeHierarchy = TypeHierarchyHelper.GetTypeHierarchy(client.GetType()); //, qualified);
			List<string> types = typeHierarchy.Keys;
			int typeCount = types.Count;

			//Debug.Log("typeCount for " + client + ": " + typeHierarchy);

			List<StyleDeclaration> classDecls = null;
			/*if (styleManager.TypeSelectorCache.ContainsKey(className))
				classDecls = styleManager.TypeSelectorCache[className];*/
			/*if (null != classDecls)
				return classDecls;*/

			classDecls = new List<StyleDeclaration>();

			// Loop over the type hierarhcy starting at the base type and work
			// down the chain of subclasses.
			for (int i = typeCount - 1; i >= 0; i--)
			{
				string type = types[i]; //.toString(); && TODO??

				//Debug.Log("Getting decls for " + type);
				List<StyleDeclaration> decls = styleManager.GetStyleDeclarations(type);
				if (null != decls)
				{
					//Debug.Log("  ->" + decls.Count);
					var matchingDecls = MatchStyleDeclarations(decls, client);
					if (null != matchingDecls)
						classDecls.AddRange(matchingDecls);
				}
			}

			#region Monitor

//#if DEBUG
//            if (null != TYPE_TO_MONITOR)
//            {
//                if (client.GetType() == TYPE_TO_MONITOR)
//                    Debug.Log(string.Format(@"### {0} classDecls ###
//{1}", TYPE_TO_MONITOR, ListUtil<StyleDeclaration>.Format(classDecls)));
//            }
//#endif

			#endregion

			classDecls = SortOnSpecificity(classDecls); //classDecls.Sort(SpecificitySort); // = SortOnSpecificity(classDecls);

			#region Monitor

//#if DEBUG
//            if (null != TYPE_TO_MONITOR)
//            {
//                if (client.GetType() == TYPE_TO_MONITOR)
//                    Debug.Log(string.Format(@"### {0} classDecls 2 ###
//{1}", TYPE_TO_MONITOR, ListUtil<StyleDeclaration>.Format(classDecls)));
//            }
//#endif

			#endregion

			#region Always advanced - NO caching!

/*if (styleManager.HasAdvancedSelectors()/* && advancedObject != null#1#)
			{
				// Advanced selectors may result in more than one match per type so
				// we sort based on specificity, but we preserve the declaration
				// order for equal selectors.
				classDecls = SortOnSpecificity(classDecls);
			}
			else
			{
				// Cache the simple type declarations for this class 
				styleManager.TypeSelectorCache[className] = classDecls;
			}*/

			#endregion


			return classDecls;
		}

		/**
		 *  
		 *  Sort algorithm to order style declarations by specificity. Note that 
		 *  Array.sort() is not used as it does not employ a stable algorithm and
		 *  CSS requires the order of equal style declaration to be preserved.
		 */ 
		private static List<StyleDeclaration> SortOnSpecificity(List<StyleDeclaration> decls) // of StyleDeclaration 
		{
			var len = decls.Count;

			if (len <= 1)
				return decls;

			for (int i = 1; i < len; i++)
			{
				for (int j = i; j > 0; j--)
				{
					if (decls[j].Specificity < decls[j-1].Specificity)
					{
						var tmp = decls[j];
						decls[j] = decls[j-1];
						decls[j-1] = tmp;
					}
					else
					{
						break;
					}
				}
			}

			return decls; 
		}

		internal static void InitProtoChain(IStyleClient client)
		{
			/*if (client.GetType().FullName == "Assets.eDriven.Skins.ImageButtonSkin")
				Debug.Log("Assets.eDriven.Skins.ImageButtonSkin: " + client);*/

			//Debug.Log("InitProtoChain: " + client);
			StyleManager styleManager = StyleManager.Instance;
			//StyleDeclaration classSelector = null;

			Component uicObject = client as Component;
			StyleDeclaration styleDeclaration;

			List<StyleDeclaration> universalSelectors = new List<StyleDeclaration>();
			bool hasStyleName = false;
			object styleName = client.StyleName;

			//Debug.LogWarning("styleName: " + styleName);

			/**
			 * prvo gledamo classname setiran na ovoj komponenti
			 * ako je setiran, uzimamo style declaration za taj classname iz poola
			 * */

			if (null != styleName)
			{
				/*if (client.StyleName is string && (string)client.StyleName == "test")
					Debug.Log("client.StyleName");*/

				// Get the style sheet referenced by the styleName property
				//classSelector = StyleManager.Instance.GetStyleDeclaration("." + StyleName);
				if (styleName is StyleDeclaration)
				{
					//Debug.LogWarning("Style name is StyleDeclaration: " + styleName);
					// Get the styles referenced by the styleName property.
					universalSelectors.Add((StyleDeclaration)styleName);
				}

				#region Still not used. This is normally used for parent-child style propagation (skins)
				//else if (/*styleName is IFlexDisplayObject || */styleName is IStyleClient) // TODO
				//{
				//    // If the styleName property is a Component, then there's a
				//    // special search path for that case.
				//    StyleProtoChain.InitProtoChainForUIComponentStyleName(client);
				//    return;
				//}
				#endregion

				else if (styleName is string)
				{
					//Debug.LogWarning("Style name is istring: " + styleName);
					hasStyleName = true;
				}
				else
				{
					Debug.LogWarning("Error");
				}
			}

			/**
			 * ovdje sada imamo style declaration za classname
			
			 * nakon toga moramo uzeti non-inherit chain (global root) i 
			 * inherit chain (inheriting styles od parenta)
			 * oni su nam potrebni kako bi prenijeli stilove iz parenta ukoliko nisu deklarirani na ovoj komponenti		
			 * */

			// To build the proto chain, we start at the end and work forward.
			// Referring to the list at the top of this function, we'll start by
			// getting the tail of the proto chain, which is:
			//  - for non-inheriting styles, the global style sheet
			//  - for inheriting styles, my parent's style object
			StyleTable nonInheritChain = styleManager.StylesRoot;
			StyleTable inheritChain;

			//IStyleClient p = Parent as IStyleClient;
			IStyleClient p = null;
			var visual = client as IVisualElement;
			if (null != visual)
				p = visual.Parent as IStyleClient;

			if (null != p)
			{
				inheritChain = p.InheritingStyles;

				#region Monitor

//#if DEBUG
//                if (null != TYPE_TO_MONITOR)
//                {
//                    if (uicObject.GetType() == TYPE_TO_MONITOR)
//                    {
//                        StringBuilder sb = new StringBuilder();
//                        sb.AppendLine(client + " -> parent chains:");
//                        sb.AppendLine("p.InheritingStyles: " + p.InheritingStyles);
//                        sb.AppendLine("p.NonInheritingStyles: " + p.NonInheritingStyles);
//                        Debug.Log(sb);
//                    }
//                }
//#endif

				#endregion

				if (inheritChain == STYLE_UNINITIALIZED)
				{
					// ako parent nema inicijaliziran inherit chain, znači da ništa ne nasljeđuje od svojih parenta
					// te također ni on sam ne definira niti jedan inheriting style.
					// u tom slučaju se možemo referencirati na non-inherit chain
					inheritChain = nonInheritChain;
				}
			}
			else
			{
				inheritChain = styleManager.StylesRoot;
			}

			#region Monitor

//#if DEBUG
//            if (null != TYPE_TO_MONITOR)
//            {
//                if (uicObject.GetType() == TYPE_TO_MONITOR)
//                {
//                    StringBuilder sb = new StringBuilder();
//                    sb.AppendLine(client + " -> chains:");
//                    sb.AppendLine("inheritChain: " + inheritChain);
//                    sb.AppendLine("nonInheritChain: " + nonInheritChain);
//                    Debug.Log(sb);
//                }
//            }
//#endif

			#endregion

			/**
			 * Sada moramo obraditi type deklaracije
			 * Radi se o tome da stil Buttona nasljeđuje i stilove definirane na superklasama
			 * Znači prvo potrebno je izbildati niz deklaracija (gleda se da li postoje definirane u stylesheetu)
			 * i to redoslijedom od superklasa do ove klase
			 * To odrađuje metoda "getClassStyleDeclarations()" koja vraća taj mini-niz
			 * Redoslijed u nizu je bitan jer propertyji definirani na subklasi overrajdaju one na superklasi
			 * */

			// Working backwards up the list, the next element in the
			// search path is the type selector

			List<StyleDeclaration> styleDeclarations = GetMatchingStyleDeclarations(client, universalSelectors);

			if (null != styleDeclarations)
			{
				#region Monitor

				if (StyleDebugging.DebugComponents.Contains(client.GetType()))
				{
					StyleDebugging.DebugDeclarationApplication(client, styleDeclarations);
				}

				#endregion

				int n = styleDeclarations.Count; //typeSelectors.Count;
				for (int i = 0; i < n; i++)
				{
					styleDeclaration = styleDeclarations[i];
					inheritChain = styleDeclaration.AddStyleToProtoChain(inheritChain, uicObject);
					nonInheritChain = styleDeclaration.AddStyleToProtoChain(nonInheritChain, uicObject);
				}
			}

			#region For simple (string) styles (like ".stile") - not used here

			//int n;

			//if (hasStyleName)
			//{
			//    var styleNames = Regex.Split((string) styleName, @"/\s+/");
			//    n = styleNames.Count();
			//    for (var i = 0; i < n; i++)
			//    {
			//        if (styleNames[i].Length > 0)
			//        {
			//            styleDeclaration = styleManager.GetMergedStyleDeclaration("." + styleNames[i]);
			//            if (null != styleDeclaration)
			//                universalSelectors.Add(styleDeclaration);
			//        }
			//    }
			//}

			//// Working backwards up the list, the next element in the
			//// search path is the type selector
			//var styleDeclarations = client.GetClassStyleDeclarations();
			////Debug.Log("##### styleDeclarations: " + styleDeclarations.Count);

			////if (client is Skin && ((Skin)client).Parent is Button)
			///*if (client.GetType().FullName == "Assets.eDriven.Skins.ImageButtonSkin")
			//    Debug.Log("Skin: " + client);*/

			//if (null != styleDeclarations)
			//{
			//    n = styleDeclarations.Count; //typeSelectors.Count;
			//    for (int i = 0; i < n; i++)
			//    {
			//        styleDeclaration = styleDeclarations[i];
			//        inheritChain = styleDeclaration.AddStyleToProtoChain(inheritChain, uicObject);
			//        nonInheritChain = styleDeclaration.AddStyleToProtoChain(nonInheritChain, uicObject);
			//        /*if (styleDeclaration.effects)
			//            object.registerEffects(styleDeclaration.effects);*/
			//    }
			//}

			//// Next are the class selectors
			//n = universalSelectors.Count;
			//for (var i = 0; i < n; i++)
			//{
			//    styleDeclaration = universalSelectors[i];
			//    if (null != styleDeclaration)
			//    {
			//        inheritChain = styleDeclaration.AddStyleToProtoChain(inheritChain, uicObject);
			//        nonInheritChain = styleDeclaration.AddStyleToProtoChain(nonInheritChain, uicObject);
			//        /*if (styleDeclaration.effects)
			//            object.registerEffects(styleDeclaration.effects);*/
			//    }
			//}

			#endregion
			
			// Finally, we'll add the in-line styles
			// to the head of the proto chain.

			styleDeclaration = client.StyleDeclaration;

			client.InheritingStyles =
				null != styleDeclaration ?
				styleDeclaration.AddStyleToProtoChain(inheritChain, uicObject) :
				inheritChain;

			client.NonInheritingStyles =
				null != styleDeclaration ?
				styleDeclaration.AddStyleToProtoChain(nonInheritChain, uicObject) :
				nonInheritChain;

			#region Monitor

//#if DEBUG
//            if (null != TYPE_TO_MONITOR)
//            {
//                if (uicObject.GetType() == TYPE_TO_MONITOR || uicObject.GetType() == typeof(HGroup))
//                {
//                    StringBuilder sb = new StringBuilder();
//                    sb.AppendLine(@"### proto chain initialized ###
//" + ComponentUtil.PathToString(uicObject, "->"));
//                    sb.AppendLine();

//                    if (null != client.StyleDeclaration && null != client.StyleDeclaration.Overrides)
//                    {
//                        sb.AppendLine("Overrides: " + client.StyleDeclaration.Overrides);
//                    }
//                    sb.AppendLine();

//                    sb.AppendLine("InheritingStyles: " + client.InheritingStyles);
//                    sb.AppendLine();

//                    sb.AppendLine("NonInheritingStyles: " + client.NonInheritingStyles);

//                    Debug.Log(sb);
//                }
//            }
//#endif

			#endregion

		}

		/**
		 *    
		 *  Find all matching style declarations for an IAdvancedStyleClient
		 *  component. The result is sorted in terms of specificity, but the
		 *  declaration order is preserved.
		 *
		 *  Param: object - an IAdvancedStyleClient instance of the component to
		 *  match.
		 *  Param: styleDeclarations - an optional Array of additional
		 *  CSSStyleDeclarations to be included in the sorted matches.
		 *
		 *  Returns: An Array of matching style declarations sorted by specificity.
		 */
		private static List<StyleDeclaration> GetMatchingStyleDeclarations(IStyleClient client, List<StyleDeclaration> styleDeclarations)
		{
			/*if (client.GetType().FullName == "Assets.eDriven.Skins.ImageButtonSkin")
				Debug.Log("GetMatchingStyleDeclarations for: " + client);*/

			StyleManager styleManager = StyleManager.Instance;
			
			if (null == styleDeclarations)
				styleDeclarations = new List<StyleDeclaration>();

			// First, look for universal selectors
			List<StyleDeclaration> universalDecls = styleManager.GetStyleDeclarations("*");/* ??
													   new List<StyleDeclaration>(); // TODO: "*"*/

			List<StyleDeclaration> list = MatchStyleDeclarations(universalDecls, client);
			styleDeclarations.AddRange(list);

			// Next, look for type selectors (includes ActionScript supertype matches)
			// If we also had universal selectors, concatenate them with our type
			// selectors and then resort by specificity...
			if (styleDeclarations.Count > 0)
			{
				//Debug.Log("first");
				list = client.GetClassStyleDeclarations();
				list.AddRange(styleDeclarations);
				styleDeclarations = list;
				styleDeclarations = SortOnSpecificity(styleDeclarations);
			}
			else
			{
				//Debug.Log("second");
				// Otherwise, we only have type selectors (which are already sorted)
				styleDeclarations = client.GetClassStyleDeclarations();

				#region Monitor

//#if DEBUG
//                if (null != TYPE_TO_MONITOR)
//                {
//                    if (client.GetType() == TYPE_TO_MONITOR)
//                        Debug.Log(string.Format(@"***  {0}:
//{1}", TYPE_TO_MONITOR, ListUtil<StyleDeclaration>.Format(styleDeclarations)));
//                }
//#endif

				#endregion

			}
			
			return styleDeclarations;
		}
		
		private static List<StyleDeclaration> MatchStyleDeclarations(List<StyleDeclaration> declarations, IStyleClient client)
		{
			//Debug.Log("declarations: " + declarations);
			//if (null == declarations)
			//    return null;

			var matchingDecls = new List<StyleDeclaration>();
			if (null == declarations)
				return matchingDecls;

			// Find the subset of declarations that match this component
			foreach (StyleDeclaration decl in declarations)
			{
				if (decl.MatchesStyleClient(client))
					matchingDecls.Add(decl);
			}

			return matchingDecls;
		}

		internal static void StyleChanged(IInvalidating client, string styleProp)
		{
			StyleManager styleManager = StyleManager.Instance;

			/**
			 * 1. Check to see if this is one of the style properties
			 * that is known to affect layout.
			 * */
			if (null == styleProp || // null refreshes the object
				styleProp == "styleName" || // mapper changed
				styleManager.IsSizeInvalidatingStyle(styleProp)) // component size depends on this style
			{
				// This style property change may affect the layout of this
				// object. Signal the LayoutManager to re-measure the object.
				client.InvalidateSize();
			}

			/**
			 * 2. This is very important:
			 * It invalidates the component display list upon each style change
			 * UpdateDisplayList will be run in the next frame
			 * This means that we can read all the available styles and manipulate children (for instance in skins, see the PanelSkin)
			 * */
			client.InvalidateDisplayList();
		
			/**
			 * 3. Check if we should invalidate parent
			 * */
			IInvalidating parent = null;
			if (client is IVisualElement)
				parent = ((IVisualElement)client).Parent as IInvalidating;

			if (null != parent)
			{
				if (styleProp == "styleName" || styleManager.IsParentSizeInvalidatingStyle(styleProp))
					parent.InvalidateSize();

				if (styleProp == "styleName" || styleManager.IsParentDisplayListInvalidatingStyle(styleProp))
					parent.InvalidateDisplayList();
			}
		}

		internal static bool MatchesCSSType(Type clientType, string cssType)
		{
			//StyleManager styleManager = StyleManager.Instance;
			//bool qualified = styleManager.QualifiedTypeSelectors;
			OrderedObject<bool> typeHierarchy = TypeHierarchyHelper.GetTypeHierarchy(clientType);
			return typeHierarchy.KeyExists(cssType);
		}

		/// <summary>
		/// Sets style
		/// </summary>
		/// <param name="client"></param>
		/// <param name="styleProp"></param>
		/// <param name="newValue"></param>
		public static void SetStyle(IStyleClient client, string styleProp, object newValue)
		{
			if (styleProp == "styleName")
			{
				// Let the setter handle this one, see Component.
				client.StyleName = styleProp;

				// Short circuit, because styleName isn't really a style.
				return;
			}

			bool isInheritingStyle = StyleManager.Instance.IsInheritingStyle(styleProp);
			bool isProtoChainInitialized = client.InheritingStyles != STYLE_UNINITIALIZED;
			bool valueChanged = client.GetStyle(styleProp) != newValue;

			/**
			 * Čim setiramo stil na komponenti pomoću fje. setStyle(), to zahtijeva specijalnu deklaraciju vezanu uz komponentu
			 * Deklaraciju inicijaliziramo ovdje
			 * */
			if (null == client.StyleDeclaration)
			{
				client.StyleDeclaration = new StyleDeclaration(null);

				client.StyleDeclaration.SetLocalStyle(styleProp, newValue);

				// If inheritingStyles is undefined, then this object is being
				// initialized and we haven't yet generated the proto chain.  To
				// avoid redundant work, don't bother to create the proto chain here.
				if (isProtoChainInitialized)
				{
					client.RegenerateStyleCache(isInheritingStyle);
				}
			}
			else
			{
				client.StyleDeclaration.SetLocalStyle(styleProp, newValue);
			}

			if (isProtoChainInitialized && valueChanged)
			{
				client.StyleChanged(styleProp);
				client.NotifyStyleChangeInChildren(styleProp, newValue, isInheritingStyle);
			}
		}
	}
}