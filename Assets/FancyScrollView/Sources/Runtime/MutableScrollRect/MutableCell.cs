/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using UnityEngine;

namespace FancyScrollView
{
    public abstract class MutableCell<TItemData, TContext> : MonoBehaviour where TContext : class, new()
    {
        public int Index = -1;
        
        public float CellSize { get; set; } = 100f;

        public virtual bool IsVisible => gameObject.activeSelf;

        protected TContext Context { get; private set; }

        public virtual void SetContext(TContext context) => Context = context;

        public virtual void Initialize() { }

        public virtual void SetVisible(bool visible) => gameObject.SetActive(visible);

        public abstract void UpdateContent(TItemData itemData);

        public abstract void UpdatePosition(float position);

        public virtual void UpdateSize(float cellSize,bool forceUpdate = false){ }

        public virtual void Destroy() => DestroyImmediate(gameObject);
    }

    /// <summary>
    /// <see cref="FancyScrollView{TItemData}"/> のセルを実装するための抽象基底クラス.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <seealso cref="FancyCell{TItemData, TContext}"/>
    public abstract class MutableCell<TItemData> : FancyCell<TItemData, NullContext>
    {
        /// <inheritdoc/>
        public sealed override void SetContext(NullContext context) => base.SetContext(context);
    }
}
