using UnityEngine;
using UnityEngine.UI;

public class WindManager : MonoBehaviour
{
    public static float windForce = 0f;


    private float maxWind = 10;

    public Image r_windArrow;
    public Image l_windArrow;
    public Image r_arrow;
    public Image l_arrow;

    void Start()
    {
        ChangeWind();
        ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");
        float tempmaxWind;
        if (float.TryParse(db.data.WindValues.WindForce, out tempmaxWind))
        {
            maxWind = tempmaxWind;
        }
        
    }

    public void ChangeWind()
    {

        windForce = Random.Range(-maxWind, maxWind);

       // Debug.Log("Wind Force: " + windForce);

        if (r_windArrow != null && l_windArrow != null)
        {
            if (windForce > 0)
            {
                r_windArrow.gameObject.SetActive(true);
                l_windArrow.gameObject.SetActive(false);
                r_arrow.gameObject.SetActive(true);
                l_arrow.gameObject.SetActive(false);

                r_windArrow.rectTransform.localScale = new Vector3(Mathf.Sign(windForce), 1, 1);
                r_windArrow.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
                r_windArrow.fillAmount = Mathf.Abs(windForce) / 10f; 
            }
            else if (windForce < 0)
            {
                r_arrow.gameObject.SetActive(false);
                l_arrow.gameObject.SetActive(true);
                r_windArrow.gameObject.SetActive(false);
                l_windArrow.gameObject.SetActive(true);

                l_windArrow.rectTransform.localScale = new Vector3(Mathf.Sign(-windForce), 1, 1);
                l_windArrow.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
                l_windArrow.fillAmount = Mathf.Abs(windForce) / 10f; 
            }
            else
            {
                r_arrow.gameObject.SetActive(false);
                l_arrow.gameObject.SetActive(false);
                r_windArrow.gameObject.SetActive(false);
                l_windArrow.gameObject.SetActive(false);
            }
        }
    }
}
