using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LoadingScreenService_DataClass : DataClass
{
    //public GameObject LoadingScreenUI;
    //public TextMeshProUGUI loadingText;
    //public GameObject loadingBar;
    //public TextMeshProUGUI barMessage;
    //public Image loadingBarFill;
    //public Image downloadingBarFill;

    public GameObject LoadingScreenUI;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI loadingText_Secondary;
    public LoadingDialog loadingDialog;

}


public class LoadingScreenService : BaseService<LoadingScreenService_DataClass>
{
    private float downloadValue;

    public override void Kill() { }

    public override void Update() { }

    public void SetLoadingBarActive(bool value, string message = "")
    {
        data.loadingText.text = message;
        data.loadingDialog.gameObject.SetActive(value);
    }

    public void SetLoadingBarProgress(float value, bool force = false)
    {
        if (value <= 0 || value > 1)
        {
            data.loadingDialog.UpdateValue(0f);
            return;
        }

        //if (!force && data.loadingDialog.value >= downloadValue * value)
        //{
        //    return;
        //}

        data.loadingDialog.UpdateValue(value);

        //if(value <= 0 || value > 1)
        //{
        //    data.loadingBarFill.fillAmount = 0f;
        //    return;
        //}

        //if (!force && data.loadingBarFill.fillAmount >= data.downloadingBarFill.fillAmount * value)
        //{
        //    return;
        //}

        //data.loadingBarFill.fillAmount = data.downloadingBarFill.fillAmount * value;
    }

    public void SetDownloadingBarProgress(float value)
    {
        downloadValue = value;
        //data.downloadingBarFill.DOFillAmount(value, 0.1f);
    }

    public void SetLoadingScreenActive(bool value, string message = "")
    {
        data.LoadingScreenUI.SetActive(value);

        if(!message.IsNullOrEmpty())
        {
            SetLoadingMessage(message);
        }
    }

    public void SetLoadingMessage(string message)
    {
        data.loadingText.text = message;
    }

    public void SetLoadingMessage_Secondary(string message)
    {
        data.loadingText_Secondary.text = message;
    }
}
