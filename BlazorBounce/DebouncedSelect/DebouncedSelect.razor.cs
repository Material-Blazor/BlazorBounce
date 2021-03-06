﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBounce
{
    /// <summary>
    /// A Material Theme select.
    /// </summary>
    public partial class DebouncedSelect<TItem> : DebouncedValidatingInputComponentFoundation<TItem>
    {
#nullable enable annotations
        /// <summary>
        /// A function delegate to return the parameters for <c>@key</c> attributes. If unused
        /// "fake" keys set to GUIDs will be used.
        /// </summary>
        [Parameter] public Func<TItem, object> GetKeysFunc { get; set; }


        /// <summary>
        /// The item list to be represented as a select
        /// </summary>
        [Parameter] public IEnumerable<MBListElement<TItem>> Items { get; set; }


        /// <summary>
        /// The form of validation to apply when Value is first set, deciding whether to accept
        /// a value outside the <see cref="Items"/> list, replace it with the first list item or
        /// to throw an exception (the default).
        /// <para>Overrides <see cref="MBCascadingDefaults.ItemValidation"/></para>
        /// </summary>
        [Parameter] public MBItemValidation ItemValidation { get; set; }


        /// <summary>
        /// The select's label.
        /// </summary>
        [Parameter] public string Label { get; set; }


        /// <summary>
        /// The select's <see cref="MBSelectInputStyle"/>.
        /// <para>Overrides <see cref="MBCascadingDefaults.SelectInputStyle"/></para>
        /// </summary>
        [Parameter] public MBSelectInputStyle SelectInputStyle { get; set; }


        /// <summary>
        /// The select's <see cref="MBTextAlignStyle"/>.
        /// <para>Overrides <see cref="MBCascadingDefaults.TextAlignStyle"/></para>
        /// </summary>
        [Parameter] public MBTextAlignStyle? TextAlignStyle { get; set; }


        /// <summary>
        /// The leading icon's name. No leading icon shown if not set.
        /// </summary>
        [Parameter] public string? LeadingIcon { get; set; }


        /// <summary>
        /// The select's density.
        /// </summary>
        [Parameter] public MBDensity? Density { get; set; }
#nullable restore annotations


        private readonly string labelId = Utilities.GenerateUniqueElementName();
        private readonly string listboxId = Utilities.GenerateUniqueElementName();
        private readonly string selectedTextId = Utilities.GenerateUniqueElementName();


        private string FloatingLabelClass { get; set; } = "";
        private Dictionary<TItem, MBListElement<TItem>> ItemDict { get; set; }
        private Func<TItem, object> KeyGenerator { get; set; }
        private DotNetObjectReference<DebouncedSelect<TItem>> ObjectReference { get; set; }
        private ElementReference SelectReference { get; set; }
        private string SelectedText { get; set; } = "";
        private bool ShowLabel => !string.IsNullOrWhiteSpace(Label);


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ItemDict = Items.ToDictionary(i => i.SelectedValue);

            ComponentValue = ValidateItemList(ItemDict.Values, ItemValidation);

            ClassMapper
                .Add("mdc-select")
                .AddIf("mdc-select--no-label", () => !ShowLabel)
                .AddIf("mdc-select--with-leading-icon", () => !string.IsNullOrWhiteSpace(LeadingIcon));

            SelectedText = (Value is null) ? "" : Items.Where(i => object.Equals(i.SelectedValue, Value)).FirstOrDefault().Label;
            FloatingLabelClass = string.IsNullOrWhiteSpace(SelectedText) ? "" : "mdc-floating-label--float-above";

            SetComponentValue += OnValueSetCallback;
            OnDisabledSet += OnDisabledSetCallback;

            ObjectReference = DotNetObjectReference.Create(this);
        }


        // Would like to use <inheritdoc/> however DocFX cannot resolve to references outside Material.Blazor
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            KeyGenerator = GetKeysFunc ?? delegate (TItem item) { return item; };
        }


        private bool _disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                ObjectReference?.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }


        /// <summary>
        /// For Material Theme to notify of menu item selection via JS Interop.
        /// </summary>
        [JSInvokable("NotifySelectedAsync")]
        public async Task NotifySelectedAsync(int index)
        {
            ComponentValue = ItemDict.Values.ElementAt(index).SelectedValue;
            await Task.CompletedTask;
        }


        /// <summary>
        /// Callback for value the value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValueSetCallback(object sender, EventArgs e) => InvokeAsync(() => JsRuntime.InvokeVoidAsync("MaterialBlazor.MBSelect.setIndex", SelectReference, ItemDict.Keys.ToList().IndexOf(Value)));


        /// <summary>
        /// Callback for value the Disabled value setter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDisabledSetCallback(object sender, EventArgs e) => InvokeAsync(() => JsRuntime.InvokeVoidAsync("MaterialBlazor.MBSelect.setDisabled", SelectReference, Disabled));


        /// <inheritdoc/>
        private protected override async Task InitiateMcwComponent()
        {
            await JsRuntime.InvokeVoidAsync("MaterialBlazor.MBSelect.init", SelectReference, ObjectReference);
            Console.WriteLine("Initiated");
        }
    }
}
