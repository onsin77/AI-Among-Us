using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PolyAndCode.UI;

public class RecyclableScrollRect : ScrollRect
{
	public enum DirectionType
	{
		Vertical,
		Horizontal
	}

	[HideInInspector]
	public IRecyclableScrollRectDataSource DataSource;

	public bool IsGrid;

	public RectTransform PrototypeCell;

	public bool SelfInitialize = true;

	public DirectionType Direction;

	[SerializeField]
	private int _segments;

	private RecyclingSystem _recyclingSystem;

	private Vector2 _prevAnchoredPos;

	public int Segments
	{
		get
		{
			return _segments;
		}
		set
		{
			_segments = Math.Max(value, 2);
		}
	}

	public IEnumerator<ICell> GetCells()
	{
		return _recyclingSystem.GetCells();
	}

	protected override void Start()
	{
		((ScrollRect)this).vertical = true;
		((ScrollRect)this).horizontal = false;
		if (Application.isPlaying && SelfInitialize)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (Direction == DirectionType.Vertical)
		{
			_recyclingSystem = new VerticalRecyclingSystem(PrototypeCell, ((ScrollRect)this).viewport, ((ScrollRect)this).content, DataSource, IsGrid, Segments);
		}
		else if (Direction == DirectionType.Horizontal)
		{
			_recyclingSystem = new HorizontalRecyclingSystem(PrototypeCell, ((ScrollRect)this).viewport, ((ScrollRect)this).content, DataSource, IsGrid, Segments);
		}
		((ScrollRect)this).vertical = Direction == DirectionType.Vertical;
		((ScrollRect)this).horizontal = Direction == DirectionType.Horizontal;
		_prevAnchoredPos = ((ScrollRect)this).content.anchoredPosition;
		((UnityEvent<Vector2>)(object)((ScrollRect)this).onValueChanged).RemoveListener((UnityAction<Vector2>)OnValueChangedListener);
		((MonoBehaviour)this).StartCoroutine(_recyclingSystem.InitCoroutine(() =>
		{
			((UnityEvent<Vector2>)(object)((ScrollRect)this).onValueChanged).AddListener((UnityAction<Vector2>)OnValueChangedListener);
		}));
	}

	public void Initialize(IRecyclableScrollRectDataSource dataSource)
	{
		DataSource = dataSource;
		Initialize();
	}

	public void OnValueChangedListener(Vector2 normalizedPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Vector2 direction = ((ScrollRect)this).content.anchoredPosition - _prevAnchoredPos;
		base.m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(direction);
		_prevAnchoredPos = ((ScrollRect)this).content.anchoredPosition;
	}

	public void ReloadData()
	{
		ReloadData(DataSource);
	}

	public void ReloadData(IRecyclableScrollRectDataSource dataSource)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (_recyclingSystem != null)
		{
			((ScrollRect)this).StopMovement();
			((UnityEvent<Vector2>)(object)((ScrollRect)this).onValueChanged).RemoveListener((UnityAction<Vector2>)OnValueChangedListener);
			_recyclingSystem.DataSource = dataSource;
			((MonoBehaviour)this).StartCoroutine(_recyclingSystem.InitCoroutine(() =>
			{
				((UnityEvent<Vector2>)(object)((ScrollRect)this).onValueChanged).AddListener((UnityAction<Vector2>)OnValueChangedListener);
			}));
			_prevAnchoredPos = ((ScrollRect)this).content.anchoredPosition;
		}
	}

	public void ScrollToBottom()
	{
		((UnityEvent<Vector2>)(object)((ScrollRect)this).onValueChanged).RemoveListener((UnityAction<Vector2>)OnValueChangedListener);
		((VerticalRecyclingSystem)_recyclingSystem).ScrollToEnd();
		((ScrollRect)this).verticalNormalizedPosition = 0f;
		((ScrollRect)this).StopMovement();
		((UnityEvent<Vector2>)(object)((ScrollRect)this).onValueChanged).AddListener((UnityAction<Vector2>)OnValueChangedListener);
	}
}
