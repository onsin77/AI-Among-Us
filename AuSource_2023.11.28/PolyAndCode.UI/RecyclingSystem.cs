using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyAndCode.UI;

public abstract class RecyclingSystem
{
	public IRecyclableScrollRectDataSource DataSource;

	protected RectTransform Viewport;

	protected RectTransform Content;

	protected RectTransform PrototypeCell;

	protected bool IsGrid;

	protected float MinPoolCoverage = 1.5f;

	protected int MinPoolSize = 10;

	protected float RecyclingThreshold = 0.2f;

	public abstract IEnumerator InitCoroutine(Action onInitialized = null);

	public abstract Vector2 OnValueChangedListener(Vector2 direction);

	public abstract IEnumerator<ICell> GetCells();
}
