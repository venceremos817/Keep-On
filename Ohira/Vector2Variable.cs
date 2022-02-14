using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/Variable/Vector2")]
public class Vector2Variable : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField] private Vector2 initialValue;

	[System.NonSerialized] public Vector2 value;

	public Vector2 InitialValue { get => initialValue; private set => initialValue = value; }

	public void OnAfterDeserialize()
	{
		value = InitialValue;
	}


	public void OnBeforeSerialize()
	{

	}
}