﻿using System;
using System.Linq;
using EasingCore;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class Example10 : MonoBehaviour
    {
        [SerializeField] MutableScrollView scrollView = default;
        [SerializeField] InputField paddingTopInputField = default;
        [SerializeField] InputField paddingBottomInputField = default;
        [SerializeField] InputField spacingInputField = default;
        [SerializeField] InputField dataCountInputField = default;
        [SerializeField] InputField selectIndexInputField = default;
        [SerializeField] Dropdown alignmentDropdown = default;

        private int selectIndex = 0;
        void Start()
        {
            scrollView.OnCellClicked(index => selectIndexInputField.text = index.ToString());

            paddingTopInputField.onValueChanged.AddListener(_ =>
                TryParseValue(paddingTopInputField, 0, 999, value => scrollView.PaddingTop = value));
            paddingTopInputField.text = scrollView.PaddingTop.ToString();

            paddingBottomInputField.onValueChanged.AddListener(_ =>
                TryParseValue(paddingBottomInputField, 0, 999, value => scrollView.PaddingBottom = value));
            paddingBottomInputField.text = scrollView.PaddingBottom.ToString();

            spacingInputField.onValueChanged.AddListener(_ =>
                TryParseValue(spacingInputField, 0, 100, value => scrollView.Spacing = value));
            spacingInputField.text = scrollView.Spacing.ToString();

            alignmentDropdown.AddOptions(Enum.GetNames(typeof(Alignment)).Select(x => new Dropdown.OptionData(x)).ToList());
            alignmentDropdown.onValueChanged.AddListener(_ => SelectCell());
            alignmentDropdown.value = (int)Alignment.Middle;

            selectIndexInputField.onValueChanged.AddListener(_ => SelectCell());
            selectIndexInputField.text = selectIndex.ToString();

            dataCountInputField.onValueChanged.AddListener(_ =>
                TryParseValue(dataCountInputField, 1, 99999, GenerateCells));
            dataCountInputField.text = "20";

            //scrollView.JumpTo(selectIndex);
        }

        void TryParseValue(InputField inputField, int min, int max, Action<int> success)
        {
            if (!int.TryParse(inputField.text, out int value))
            {
                return;
            }

            if (value < min || value > max)
            {
                inputField.text = Mathf.Clamp(value, min, max).ToString();
                return;
            }

            success(value);
        }

        void SelectCell()
        {
            return;
            if (scrollView.DataCount == 0)  return;

            TryParseValue(selectIndexInputField, 0, scrollView.DataCount - 1, index =>
                scrollView.ScrollTo(index, 0.3f, Ease.InOutQuint, (Alignment)alignmentDropdown.value));
        }

        void GenerateCells(int dataCount)
        {
            var items = Enumerable.Range(0, dataCount)
                .Select(i => new MutableItemData($"Cell {i}"))
                .ToArray();
            var mappings = items.Select((c,index) => new MutablePrefabMapping()
            {
                DataSourceIndex = index,
                CellSize = index %2 ==0?  100:100,
            }).ToArray();
            
            var random = new Random(2);
            foreach (var map in mappings)
            {
               map.CellSize = random.Next(100,300);
            }
 
            scrollView.UpdateData(items,mappings);
            //SelectCell();
        }
    }
}