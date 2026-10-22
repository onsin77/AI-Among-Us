using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolyAndCode.UI;

public class VerticalRecyclingSystem : RecyclingSystem
{
	private readonly int _coloumns;

	private float _cellWidth;

	private float _cellHeight;

	private List<RectTransform> _cellPool;

	private List<ICell> _cachedCells;

	private Bounds _recyclableViewBounds;

	private readonly Vector3[] _corners = new Vector3[4];

	private bool _recycling;

	private int currentItemCount;

	private int topMostCellIndex;

	private int bottomMostCellIndex;

	private int _topMostCellColoumn;

	private int _bottomMostCellColoumn;

	private Vector2 zeroVector = Vector2.zero;

	public VerticalRecyclingSystem(RectTransform prototypeCell, RectTransform viewport, RectTransform content, IRecyclableScrollRectDataSource dataSource, bool isGrid, int coloumns)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		PrototypeCell = prototypeCell;
		Viewport = viewport;
		Content = content;
		DataSource = dataSource;
		IsGrid = isGrid;
		_coloumns = ((!isGrid) ? 1 : coloumns);
		_recyclableViewBounds = default;
	}

	public override IEnumerator InitCoroutine(Action onInitialized)
	{
		SetTopAnchor(Content);
		Content.anchoredPosition = Vector2.op_Implicit(Vector3.zero);
		yield return null;
		SetRecyclingBounds();
		CreateCellPool();
		currentItemCount = _cellPool.Count;
		topMostCellIndex = 0;
		bottomMostCellIndex = _cellPool.Count - 1;
		float num = (float)(int)Mathf.Ceil((float)_cellPool.Count / (float)_coloumns) * _cellHeight;
		Content.sizeDelta = new Vector2(Content.sizeDelta.x, num);
		SetTopAnchor(Content);
		onInitialized?.Invoke();
	}

	public void ScrollToEnd()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		int itemCount = DataSource.GetItemCount();
		int count = _cellPool.Count;
		if (!(_cellHeight * (float)itemCount <= _recyclableViewBounds.size.y))
		{
			int num = bottomMostCellIndex;
			int num2 = itemCount - 1;
			for (int i = 0; i < count; i++)
			{
				DataSource.SetCell(_cachedCells[num], num2);
				num2--;
				num = ((num != 0) ? (num - 1) : (count - 1));
			}
			currentItemCount = itemCount;
			Content.anchoredPosition = Vector2.up * _recyclableViewBounds.size.y;
		}
	}

	private void SetRecyclingBounds()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		Viewport.GetWorldCorners(_corners);
		float num = RecyclingThreshold * (_corners[2].y - _corners[0].y);
		_recyclableViewBounds.min = new Vector3(_corners[0].x, _corners[0].y - num);
		_recyclableViewBounds.max = new Vector3(_corners[2].x, _corners[2].y + num);
	}

	private void CreateCellPool()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		if (_cellPool != null)
		{
			_cellPool.ForEach((RectTransform item) =>
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			});
			_cellPool.Clear();
			_cachedCells.Clear();
		}
		else
		{
			_cachedCells = new List<ICell>();
			_cellPool = new List<RectTransform>();
		}
		((Component)PrototypeCell).gameObject.SetActive(true);
		if (IsGrid)
		{
			SetTopLeftAnchor(PrototypeCell);
		}
		else
		{
			SetTopAnchor(PrototypeCell);
		}
		_topMostCellColoumn = (_bottomMostCellColoumn = 0);
		float num = 0f;
		int num2 = 0;
		float num3 = 0f;
		float num4 = 0f;
		Rect rect = Content.rect;
		_cellWidth = rect.width / (float)_coloumns;
		_cellHeight = PrototypeCell.sizeDelta.y / PrototypeCell.sizeDelta.x * _cellWidth;
		float minPoolCoverage = MinPoolCoverage;
		rect = Viewport.rect;
		float num5 = minPoolCoverage * rect.height;
		for (int num6 = Math.Min(MinPoolSize, DataSource.GetItemCount()); (num2 < num6 || num < num5) && num2 < DataSource.GetItemCount(); num2++)
		{
			RectTransform component = Object.Instantiate<GameObject>(((Component)PrototypeCell).gameObject).GetComponent<RectTransform>();
			((Object)component).name = "Cell";
			component.sizeDelta = new Vector2(_cellWidth, _cellHeight);
			_cellPool.Add(component);
			((Transform)component).SetParent((Transform)(object)Content, false);
			if (IsGrid)
			{
				num3 = (float)_bottomMostCellColoumn * _cellWidth;
				component.anchoredPosition = new Vector2(num3, num4);
				if (++_bottomMostCellColoumn >= _coloumns)
				{
					_bottomMostCellColoumn = 0;
					num4 -= _cellHeight;
					float num7 = num;
					rect = component.rect;
					num = num7 + rect.height;
				}
			}
			else
			{
				component.anchoredPosition = new Vector2(0f, num4);
				float y = component.anchoredPosition.y;
				rect = component.rect;
				num4 = y - rect.height;
				float num8 = num;
				rect = component.rect;
				num = num8 + rect.height;
			}
			_cachedCells.Add(((Component)component).GetComponent<ICell>());
			DataSource.SetCell(_cachedCells[_cachedCells.Count - 1], num2);
		}
		if (IsGrid)
		{
			_bottomMostCellColoumn = (_bottomMostCellColoumn - 1 + _coloumns) % _coloumns;
		}
		Scene scene = ((Component)PrototypeCell).gameObject.scene;
		if (scene.IsValid())
		{
			((Component)PrototypeCell).gameObject.SetActive(false);
		}
	}

	public override Vector2 OnValueChangedListener(Vector2 direction)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (_recycling || _cellPool == null || _cellPool.Count == 0)
		{
			return zeroVector;
		}
		SetRecyclingBounds();
		if (direction.y > 0f && _cellPool[bottomMostCellIndex].MaxY() > _recyclableViewBounds.min.y)
		{
			return RecycleTopToBottom();
		}
		if (direction.y < 0f && _cellPool[topMostCellIndex].MinY() < _recyclableViewBounds.max.y)
		{
			return RecycleBottomToTop();
		}
		return zeroVector;
	}

	private Vector2 RecycleTopToBottom()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		_recycling = true;
		int n = 0;
		float num = (IsGrid ? _cellPool[bottomMostCellIndex].anchoredPosition.y : 0f);
		float num2 = 0f;
		int num3 = 0;
		while (_cellPool[topMostCellIndex].MinY() > _recyclableViewBounds.max.y && currentItemCount < DataSource.GetItemCount())
		{
			if (IsGrid)
			{
				if (++_bottomMostCellColoumn >= _coloumns)
				{
					n++;
					_bottomMostCellColoumn = 0;
					num = _cellPool[bottomMostCellIndex].anchoredPosition.y - _cellHeight;
					num3++;
				}
				num2 = (float)_bottomMostCellColoumn * _cellWidth;
				_cellPool[topMostCellIndex].anchoredPosition = new Vector2(num2, num);
				if (++_topMostCellColoumn >= _coloumns)
				{
					_topMostCellColoumn = 0;
					num3--;
				}
			}
			else
			{
				num = _cellPool[bottomMostCellIndex].anchoredPosition.y - _cellPool[bottomMostCellIndex].sizeDelta.y;
				_cellPool[topMostCellIndex].anchoredPosition = new Vector2(_cellPool[topMostCellIndex].anchoredPosition.x, num);
			}
			DataSource.SetCell(_cachedCells[topMostCellIndex], currentItemCount);
			bottomMostCellIndex = topMostCellIndex;
			topMostCellIndex = (topMostCellIndex + 1) % _cellPool.Count;
			currentItemCount++;
			if (!IsGrid)
			{
				n++;
			}
		}
		if (IsGrid)
		{
			RectTransform content = Content;
			content.sizeDelta += (float)num3 * Vector2.up * _cellHeight;
			if (num3 > 0)
			{
				n -= num3;
			}
		}
		_cellPool.ForEach((RectTransform cell) =>
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			cell.anchoredPosition += (float)n * Vector2.up * _cellPool[topMostCellIndex].sizeDelta.y;
		});
		RectTransform content2 = Content;
		content2.anchoredPosition -= (float)n * Vector2.up * _cellPool[topMostCellIndex].sizeDelta.y;
		_recycling = false;
		return -new Vector2(0f, (float)n * _cellPool[topMostCellIndex].sizeDelta.y);
	}

	private Vector2 RecycleBottomToTop()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		_recycling = true;
		int n = 0;
		float num = (IsGrid ? _cellPool[topMostCellIndex].anchoredPosition.y : 0f);
		float num2 = 0f;
		int num3 = 0;
		while (_cellPool[bottomMostCellIndex].MaxY() < _recyclableViewBounds.min.y && currentItemCount > _cellPool.Count)
		{
			if (IsGrid)
			{
				if (--_topMostCellColoumn < 0)
				{
					n++;
					_topMostCellColoumn = _coloumns - 1;
					num = _cellPool[topMostCellIndex].anchoredPosition.y + _cellHeight;
					num3++;
				}
				num2 = (float)_topMostCellColoumn * _cellWidth;
				_cellPool[bottomMostCellIndex].anchoredPosition = new Vector2(num2, num);
				if (--_bottomMostCellColoumn < 0)
				{
					_bottomMostCellColoumn = _coloumns - 1;
					num3--;
				}
			}
			else
			{
				num = _cellPool[topMostCellIndex].anchoredPosition.y + _cellPool[topMostCellIndex].sizeDelta.y;
				_cellPool[bottomMostCellIndex].anchoredPosition = new Vector2(_cellPool[bottomMostCellIndex].anchoredPosition.x, num);
				n++;
			}
			currentItemCount--;
			DataSource.SetCell(_cachedCells[bottomMostCellIndex], currentItemCount - _cellPool.Count);
			topMostCellIndex = bottomMostCellIndex;
			bottomMostCellIndex = (bottomMostCellIndex - 1 + _cellPool.Count) % _cellPool.Count;
		}
		if (IsGrid)
		{
			RectTransform content = Content;
			content.sizeDelta += (float)num3 * Vector2.up * _cellHeight;
			if (num3 > 0)
			{
				n -= num3;
			}
		}
		_cellPool.ForEach((RectTransform cell) =>
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			cell.anchoredPosition -= (float)n * Vector2.up * _cellPool[topMostCellIndex].sizeDelta.y;
		});
		RectTransform content2 = Content;
		content2.anchoredPosition += (float)n * Vector2.up * _cellPool[topMostCellIndex].sizeDelta.y;
		_recycling = false;
		return new Vector2(0f, (float)n * _cellPool[topMostCellIndex].sizeDelta.y);
	}

	public override IEnumerator<ICell> GetCells()
	{
		if (_cachedCells == null)
		{
			yield break;
		}
		foreach (ICell cachedCell in _cachedCells)
		{
			yield return cachedCell;
		}
	}

	private void SetTopAnchor(RectTransform rectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		float width = rect.width;
		rect = rectTransform.rect;
		float height = rect.height;
		rectTransform.anchorMin = new Vector2(0.5f, 1f);
		rectTransform.anchorMax = new Vector2(0.5f, 1f);
		rectTransform.pivot = new Vector2(0.5f, 1f);
		rectTransform.sizeDelta = new Vector2(width, height);
	}

	private void SetTopLeftAnchor(RectTransform rectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		float width = rect.width;
		rect = rectTransform.rect;
		float height = rect.height;
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(0f, 1f);
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.sizeDelta = new Vector2(width, height);
	}

	public void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.green;
		Gizmos.DrawLine(_recyclableViewBounds.min - new Vector3(2000f, 0f), _recyclableViewBounds.min + new Vector3(2000f, 0f));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(_recyclableViewBounds.max - new Vector3(2000f, 0f), _recyclableViewBounds.max + new Vector3(2000f, 0f));
	}
}
