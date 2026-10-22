using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolyAndCode.UI;

public class HorizontalRecyclingSystem : RecyclingSystem
{
	private readonly int _rows;

	private float _cellWidth;

	private float _cellHeight;

	private List<RectTransform> _cellPool;

	private List<ICell> _cachedCells;

	private Bounds _recyclableViewBounds;

	private readonly Vector3[] _corners = new Vector3[4];

	private bool _recycling;

	private int currentItemCount;

	private int leftMostCellIndex;

	private int rightMostCellIndex;

	private int _leftMostCellRow;

	private int _RightMostCellRow;

	private Vector2 zeroVector = Vector2.zero;

	public HorizontalRecyclingSystem(RectTransform prototypeCell, RectTransform viewport, RectTransform content, IRecyclableScrollRectDataSource dataSource, bool isGrid, int rows)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		PrototypeCell = prototypeCell;
		Viewport = viewport;
		Content = content;
		DataSource = dataSource;
		IsGrid = isGrid;
		_rows = ((!isGrid) ? 1 : rows);
		_recyclableViewBounds = default;
	}

	public override IEnumerator InitCoroutine(Action onInitialized)
	{
		SetLeftAnchor(Content);
		Content.anchoredPosition = Vector2.op_Implicit(Vector3.zero);
		yield return null;
		SetRecyclingBounds();
		CreateCellPool();
		currentItemCount = _cellPool.Count;
		leftMostCellIndex = 0;
		rightMostCellIndex = _cellPool.Count - 1;
		float num = (float)Mathf.CeilToInt((float)_cellPool.Count / (float)_rows) * _cellWidth;
		Content.sizeDelta = new Vector2(num, Content.sizeDelta.y);
		SetLeftAnchor(Content);
		onInitialized?.Invoke();
	}

	private void SetRecyclingBounds()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		Viewport.GetWorldCorners(_corners);
		float num = RecyclingThreshold * (_corners[2].x - _corners[0].x);
		_recyclableViewBounds.min = new Vector3(_corners[0].x - num, _corners[0].y);
		_recyclableViewBounds.max = new Vector3(_corners[2].x + num, _corners[2].y);
	}

	private void CreateCellPool()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
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
		SetLeftAnchor(PrototypeCell);
		Rect rect = Content.rect;
		_cellHeight = rect.height / (float)_rows;
		_cellWidth = PrototypeCell.sizeDelta.x / PrototypeCell.sizeDelta.y * _cellHeight;
		_leftMostCellRow = (_RightMostCellRow = 0);
		float num = 0f;
		int num2 = 0;
		float num3 = 0f;
		float num4 = 0f;
		float minPoolCoverage = MinPoolCoverage;
		rect = Viewport.rect;
		float num5 = minPoolCoverage * rect.width;
		for (int num6 = Math.Min(MinPoolSize, DataSource.GetItemCount()); (num2 < num6 || num < num5) && num2 < DataSource.GetItemCount(); num2++)
		{
			RectTransform component = Object.Instantiate<GameObject>(((Component)PrototypeCell).gameObject).GetComponent<RectTransform>();
			((Object)component).name = "Cell";
			component.sizeDelta = new Vector2(_cellWidth, _cellHeight);
			_cellPool.Add(component);
			((Transform)component).SetParent((Transform)(object)Content, false);
			if (IsGrid)
			{
				num4 = (float)(-_RightMostCellRow) * _cellHeight;
				component.anchoredPosition = new Vector2(num3, num4);
				if (++_RightMostCellRow >= _rows)
				{
					_RightMostCellRow = 0;
					num3 += _cellWidth;
					float num7 = num;
					rect = component.rect;
					num = num7 + rect.width;
				}
			}
			else
			{
				component.anchoredPosition = new Vector2(num3, 0f);
				float x = component.anchoredPosition.x;
				rect = component.rect;
				num3 = x + rect.width;
				float num8 = num;
				rect = component.rect;
				num = num8 + rect.width;
			}
			_cachedCells.Add(((Component)component).GetComponent<ICell>());
			DataSource.SetCell(_cachedCells[_cachedCells.Count - 1], num2);
		}
		if (IsGrid)
		{
			_RightMostCellRow = (_RightMostCellRow - 1 + _rows) % _rows;
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
		if (direction.x < 0f && _cellPool[rightMostCellIndex].MinX() < _recyclableViewBounds.max.x)
		{
			return RecycleLeftToRight();
		}
		if (direction.x > 0f && _cellPool[leftMostCellIndex].MaxX() > _recyclableViewBounds.min.x)
		{
			return RecycleRightToleft();
		}
		return zeroVector;
	}

	private Vector2 RecycleLeftToRight()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		_recycling = true;
		int n = 0;
		float num = (IsGrid ? _cellPool[rightMostCellIndex].anchoredPosition.x : 0f);
		float num2 = 0f;
		int num3 = 0;
		while (_cellPool[leftMostCellIndex].MaxX() < _recyclableViewBounds.min.x && currentItemCount < DataSource.GetItemCount())
		{
			if (IsGrid)
			{
				if (++_RightMostCellRow >= _rows)
				{
					n++;
					_RightMostCellRow = 0;
					num = _cellPool[rightMostCellIndex].anchoredPosition.x + _cellWidth;
					num3++;
				}
				num2 = (float)(-_RightMostCellRow) * _cellHeight;
				_cellPool[leftMostCellIndex].anchoredPosition = new Vector2(num, num2);
				if (++_leftMostCellRow >= _rows)
				{
					_leftMostCellRow = 0;
					num3--;
				}
			}
			else
			{
				num = _cellPool[rightMostCellIndex].anchoredPosition.x + _cellPool[rightMostCellIndex].sizeDelta.x;
				_cellPool[leftMostCellIndex].anchoredPosition = new Vector2(num, _cellPool[leftMostCellIndex].anchoredPosition.y);
			}
			DataSource.SetCell(_cachedCells[leftMostCellIndex], currentItemCount);
			rightMostCellIndex = leftMostCellIndex;
			leftMostCellIndex = (leftMostCellIndex + 1) % _cellPool.Count;
			currentItemCount++;
			if (!IsGrid)
			{
				n++;
			}
		}
		if (IsGrid)
		{
			RectTransform content = Content;
			content.sizeDelta += (float)num3 * Vector2.right * _cellWidth;
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
			cell.anchoredPosition -= (float)n * Vector2.right * _cellPool[leftMostCellIndex].sizeDelta.x;
		});
		RectTransform content2 = Content;
		content2.anchoredPosition += (float)n * Vector2.right * _cellPool[leftMostCellIndex].sizeDelta.x;
		_recycling = false;
		return (float)n * Vector2.right * _cellPool[leftMostCellIndex].sizeDelta.x;
	}

	private Vector2 RecycleRightToleft()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		_recycling = true;
		int n = 0;
		float num = (IsGrid ? _cellPool[leftMostCellIndex].anchoredPosition.x : 0f);
		float num2 = 0f;
		int num3 = 0;
		while (_cellPool[rightMostCellIndex].MinX() > _recyclableViewBounds.max.x && currentItemCount > _cellPool.Count)
		{
			if (IsGrid)
			{
				if (--_leftMostCellRow < 0)
				{
					n++;
					_leftMostCellRow = _rows - 1;
					num = _cellPool[leftMostCellIndex].anchoredPosition.x - _cellWidth;
					num3++;
				}
				num2 = (float)(-_leftMostCellRow) * _cellHeight;
				_cellPool[rightMostCellIndex].anchoredPosition = new Vector2(num, num2);
				if (--_RightMostCellRow < 0)
				{
					_RightMostCellRow = _rows - 1;
					num3--;
				}
			}
			else
			{
				num = _cellPool[leftMostCellIndex].anchoredPosition.x - _cellPool[leftMostCellIndex].sizeDelta.x;
				_cellPool[rightMostCellIndex].anchoredPosition = new Vector2(num, _cellPool[rightMostCellIndex].anchoredPosition.y);
				n++;
			}
			currentItemCount--;
			DataSource.SetCell(_cachedCells[rightMostCellIndex], currentItemCount - _cellPool.Count);
			leftMostCellIndex = rightMostCellIndex;
			rightMostCellIndex = (rightMostCellIndex - 1 + _cellPool.Count) % _cellPool.Count;
		}
		if (IsGrid)
		{
			RectTransform content = Content;
			content.sizeDelta += (float)num3 * Vector2.right * _cellWidth;
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
			cell.anchoredPosition += (float)n * Vector2.right * _cellPool[leftMostCellIndex].sizeDelta.x;
		});
		RectTransform content2 = Content;
		content2.anchoredPosition -= (float)n * Vector2.right * _cellPool[leftMostCellIndex].sizeDelta.x;
		_recycling = false;
		return (float)(-n) * Vector2.right * _cellPool[leftMostCellIndex].sizeDelta.x;
	}

	public override IEnumerator<ICell> GetCells()
	{
		foreach (ICell cachedCell in _cachedCells)
		{
			yield return cachedCell;
		}
	}

	private void SetLeftAnchor(RectTransform rectTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		float width = rect.width;
		rect = rectTransform.rect;
		float height = rect.height;
		Vector2 pivot = (rectTransform.anchorMax = (rectTransform.anchorMin = (IsGrid ? new Vector2(0f, 1f) : new Vector2(0f, 0.5f))));
		rectTransform.pivot = pivot;
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
		Gizmos.DrawLine(_recyclableViewBounds.min - new Vector3(0f, 2000f), _recyclableViewBounds.min + new Vector3(0f, 2000f));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(_recyclableViewBounds.max - new Vector3(0f, 2000f), _recyclableViewBounds.max + new Vector3(0f, 2000f));
	}
}
