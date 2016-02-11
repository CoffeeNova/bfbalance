using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoffeeJelly.Byfly.BFlib
{
    /// <summary>
    /// Класс для работы с формой и ее дочерними элементами.
    /// </summary>
    public static class WpfUiAssistant
    {
        /// <summary>
        /// Cписок всех wpf элементов в заданном родительском элементе.
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="Controls"></param>
        private static void ChildControls(FrameworkElement elem, List<FrameworkElement> Controls)
        {
            foreach (FrameworkElement child in LogicalTreeHelper.GetChildren(elem))
            {
                try
                {
                    Controls.Add(child);
                    if (child is ContentControl)
                    {
                        if (!((child as ContentControl).Content is string))
                            ChildControls((FrameworkElement)(child as ContentControl).Content, Controls);
                    }
                    else ChildControls(child, Controls);
                }
                catch { }
            }
        }

        /// <summary>
        /// Создает список всех  заданных дочерних элементов в родительском элементе.
        /// </summary>
        /// <param name="parentElement">Родительский (внешний) элемент.</param>
        /// <param name="childElementType">Тип дочернего (внутреннего) элемента.</param>
        /// <param name="visibility"> <see langword="true"/> - создаст список только видимых элементов. </param>
        /// <returns>Список дочерних элементов.</returns>
        public static List<FrameworkElement> ListElements(FrameworkElement parentElement, Type childElementType, bool visibility)
        {
            List<FrameworkElement> childElemList = new List<FrameworkElement>();
            List<FrameworkElement> allElem = new List<FrameworkElement>();
            //создадим список всех элементов в родительском элементе
            ChildControls(parentElement, allElem);
            //выберем из списка только видимые и нужные по типу
            foreach (FrameworkElement elem in allElem)
            {
                if (elem.GetType() == childElementType && elem.Visibility != Visibility.Hidden && visibility == true)
                    childElemList.Add(elem);
                else if (elem.GetType() == childElementType && visibility == false)
                    childElemList.Add(elem);
            }
            return childElemList;
        }

        /// <summary>
        /// Cоздает список различных заданных типов <paramref name="elementTypeList"/> элементов, в заданных списком <paramref name="AreaList"/> областях.
        /// </summary>
        /// <param name="AreasList">Список родительских областей, например компоновок типа Grid.</param>
        /// <param name="elementTypeList">Список используемых типов элементов.</param>
        /// <param name="visibility"><see langword="true"/> - создаст список только видимых элементов. </param>
        /// <returns></returns>
        /// <remarks>Например? создаст список из различных элементов (<see cref="IPTextBox"/>, <see cref="CheckBox"/>, <see cref="TextBox"/>, итд) в различных компановках (<see cref="Grid"/>, <see cref="StackPanel"/>, итд) или других родительских элементах).</remarks>
        public static List<FrameworkElement> ListElementsInAreas(List<FrameworkElement> AreasList, List<Type> elementTypeList, bool visibility)
        {
            List<FrameworkElement> allElements = new List<FrameworkElement>();
            foreach (FrameworkElement oneArea in AreasList) //перебор списка областей
            {
                foreach (Type oneElementType in elementTypeList) //перебор списка типов элементов
                {
                    allElements.AddRange(ListElements(oneArea, oneElementType, visibility));
                }
            }
            return allElements;
        }
        /// <summary>
        /// Переключает состояние <see cref="FrameworkElement.IsEnabled"/> всех заданных списком типов <paramref name="elementTypeList"/> элементов в родительском элементе <paramref name="parent"/>, в соответствии с <paramref name="iSwitch"/>.
        /// </summary>
        /// <param name="parent">Внешний элемент: контейнер, компановка итд, в дочерних элементах которого необходимо провести переключение свойства <see cref="FrameworkElement.IsEnabled"/>.</param>
        /// <param name="elementTypeList">Список типов элементов, которые попадут под переключение свойства isEnabled.</param>
        /// <param name="iSwitch">1- <see cref="FrameworkElement.IsEnabled"/>=<see langword="true"/> ; 0 - <see cref="FrameworkElement.IsEnabled"/>=<see langword="false"/>.</param>
        public static void SwitchEnabledValue(FrameworkElement parent, List<Type> elementTypeList, bool iSwitch)
        {
            List<FrameworkElement> switchingList = new List<FrameworkElement>();
            foreach (Type oneElementType in elementTypeList)
            {
                switchingList.AddRange(ListElements(parent, oneElementType, true));
            }

            foreach (FrameworkElement switchingElement in switchingList)
            {
                if (iSwitch)
                    switchingElement.IsEnabled = true;
                else
                    switchingElement.IsEnabled = false;
            }
        }
        /// <summary>
        /// Переключает состояние <see cref="FrameworkElement.IsEnabled"/> элемента типа <paramref name="elementType"/> в родительском элементе <paramref name="parent"/>, в соответствии с <paramref name="iSwitch"/>.
        /// </summary>
        /// <param name="parent">Внешний элемент: контейнер, компановка итд, в дочерних элементах которого необходимо провести переключение свойства <see cref="FrameworkElement.IsEnabled"/>.</param>
        /// <param name="elementType">Тип элемента, который попадет под переключение свойства <see cref="FrameworkElement.IsEnabled"/>.</param>
        /// <param name="iSwitch">1- <see cref="FrameworkElement.IsEnabled"/>=<see langword="true"/> ; 0 - <see cref="FrameworkElement.IsEnabled"/>=<see langword="false"/>.</param>
        public static void SwitchEnabledValue(FrameworkElement parent, Type elementType, bool iSwitch)
        {
            List<FrameworkElement> switchingList = new List<FrameworkElement>();

            switchingList.AddRange(ListElements(parent, elementType, true));
            foreach (FrameworkElement switchingElement in switchingList)
            {
                if (iSwitch)
                    switchingElement.IsEnabled = true;
                else
                    switchingElement.IsEnabled = false;
            }
        }
        /// <summary>
        /// Переключает состояние <see cref="FrameworkElement.Visibility"/> всех заданных списком типов <paramref name="elementTypeList"/> элементов в родительском элементе <paramref name="parent"/>, в соответствии с <paramref name="visibility"/>.
        /// </summary>
        /// <param name="parent">Внешний элемент: контейнер, компановка итд, в дочерних элементах которого необходимо провести переключение свойства <see cref="FrameworkElement.Visibility"/>.</param>
        /// <param name="elementTypeList">Список типов элементов, которые попадут под переключение свойства <see cref="FrameworkElement.Visibility"/>.</param>
        /// <param name="visibility">Тип определяет состояние свойства <see cref="FrameworkElement.Visibility"/> элементов</param>.
        public static void SwitchVisibleValue(FrameworkElement parent, List<Type> elementTypeList, Visibility visibility)
        {
            List<FrameworkElement> switchingList = new List<FrameworkElement>();
            foreach (Type oneElementType in elementTypeList)
            {
                switchingList.AddRange(ListElements(parent, oneElementType, true));
            }

            foreach (FrameworkElement switchingElement in switchingList)
            {
                switch (visibility)
                {
                    case Visibility.Visible:
                        switchingElement.Visibility = Visibility.Visible;
                        break;
                    case Visibility.Hidden:
                        switchingElement.Visibility = Visibility.Hidden;
                        break;
                    case Visibility.Collapsed:
                        switchingElement.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }
        /// <summary>
        /// Переключает состояние <see cref="FrameworkElement.Visibility"/> элемента типа <paramref name="elementType"/> в родительском элементе <paramref name="parent"/>, в соответствии с <paramref name="visibility"/>.
        /// </summary>
        /// <param name="parent">Внешний элемент: контейнер, компановка итд, в дочерних элементах которого необходимо провести переключение свойства <see cref="FrameworkElement.Visibility"/></param>
        /// <param name="elementType">Тип элемента, который попадет под переключение свойства <see cref="FrameworkElement.Visibility"/>.</param>
        /// <param name="visibility">Тип определяет состояние свойства <see cref="FrameworkElement.Visibility"/> элементов.</param>
        public static void SwitchVisibleValue(FrameworkElement parent, Type elementType, Visibility visibility)
        {
            List<FrameworkElement> switchingList = new List<FrameworkElement>();

            switchingList.AddRange(ListElements(parent, elementType, true));
            foreach (FrameworkElement switchingElement in switchingList)
            {
                switch (visibility)
                {
                    case Visibility.Visible:
                        switchingElement.Visibility = Visibility.Visible;
                        break;
                    case Visibility.Hidden:
                        switchingElement.Visibility = Visibility.Hidden;
                        break;
                    case Visibility.Collapsed:
                        switchingElement.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        /// <summary>
        /// Получает родительский элемент.
        /// </summary>
        /// <param name="obj">Исходный элемент.</param>
        /// <returns>Родительский элемент <see cref="DependencyObject"/>.</returns>
        /// <remarks> Стыбжено с http://code.logos.com/blog/2008/02/finding_ancestor_elements_in_w.html </remarks>
        public static DependencyObject GetParent(DependencyObject obj)
        {
            if (obj == null)
                return null;

            ContentElement ce = obj as ContentElement;
            if (ce != null)
            {
                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                FrameworkContentElement fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            return VisualTreeHelper.GetParent(obj);
        }

        /// <summary>
        /// Производит поиск предка, который удовлетворяет заданый тип <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Тип данных <see cref="DependencyObject"/></typeparam>
        /// <param name="obj"></param>
        /// <returns>Возвращает элемент типа <typeparamref name="T"/>.</returns>
        public static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                T objTest = obj as T;
                if (objTest != null)
                    return objTest;
                obj = GetParent(obj);
            }

            return null;
        }

    }
}
