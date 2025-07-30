//using GLTFast.Custom;
//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(MetadataKeyValuePair))]
//public class KeypairDrawer : PropertyDrawer
//{
//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//	{
//		float halfWidth = position.width / 2;

//		EditorGUI.BeginProperty(position, label, property);

//		var keyProp = property.FindPropertyRelative("Key");
//		var valueProp = property.FindPropertyRelative("Value");

//		Rect keyRect = new Rect(position.x, position.y, halfWidth - 5, position.height);
//		Rect valueRect = new Rect(position.x + halfWidth + 5, position.y, halfWidth - 5, position.height);

//		string keydata = keyProp.stringValue;
//		string valueData = valueProp.stringValue;

//		//EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
//		//EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

//		EditorGUI.LabelField(keyRect, keydata);
//		EditorGUI.LabelField(valueRect, valueData);

//        EditorGUI.EndProperty();
//	}
//}
