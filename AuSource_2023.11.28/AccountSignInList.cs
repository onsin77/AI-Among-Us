using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class AccountSignInList : MonoBehaviour
{
	public AccountButton ButtonPrefab;

	public Scroller ButtonParent;

	public float ButtonStart = 0.5f;

	public float ButtonHeight = 0.5f;

	private AccountButton[] AllButtons;

	public AccountsMenu parent;

	public ControllerNavMenu controllerNavParent;

	public bool createAccount;

	public void Start()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected Obj, but got Unknown
		Collider2D component = ((Component)ButtonParent).GetComponent<Collider2D>();
		string[] array = new string[4] { "Steam", "Epic", "Apple", "Google" };
		Vector3 localPosition = new Vector3(0f, ButtonStart, -0.5f);
		AllButtons = new AccountButton[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			AccountButton button = Object.Instantiate<AccountButton>(ButtonPrefab, ButtonParent.Inner);
			AllButtons[i] = button;
			((TMP_Text)button.Text).text = array[i];
			if (!createAccount)
			{
				((UnityEvent)button.Button.OnClick).AddListener((UnityAction)(() =>
				{
					LogInWith(button);
				}));
			}
			button.Button.ClickMask = component;
			((Component)button).transform.localPosition = localPosition;
			localPosition.y -= ButtonHeight;
			controllerNavParent.ControllerSelectable.Add(button.Button);
		}
		if (array.Length != 0)
		{
			controllerNavParent.DefaultButtonSelected = AllButtons[0].Button;
		}
		ButtonParent.SetBoundsMax((float)array.Length * ButtonHeight - 2f * ButtonStart - 0.1f, 0f);
	}

	public void LogInWith(AccountButton selected)
	{
	}
}
