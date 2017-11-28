﻿using UnityEngine;
using System.Collections;
using System;
using AzureServicesForUnity;
using System.Linq;
using UnityEngine.UI;
using AzureServicesForUnity.Shared;
using AzureServicesForUnity.AppService;

public class AppServiceUIScript : MonoBehaviour
{
    public Text StatusText;

    public void Start()
    {
        Globals.DebugFlag = true;

        if (Globals.DebugFlag)
            Debug.Log("instantiated Azure Services for Unity, version " + Globals.LibraryVersion);

        //get the authentication token somehow...
        //e.g. for facebook, check the Unity Facebook SDK at https://developers.facebook.com/docs/unity
        EasyAPIsClient.Instance.AuthenticationToken = "";
        EasyTablesClient.Instance.AuthenticationToken = "";

        //check here for more information regarding authentication and authorization in Azure App Service
        //https://azure.microsoft.com/en-us/documentation/articles/app-service-authentication-overview/
    }

    private void ShowError(string error)
    {
        Debug.Log(error);
        StatusText.text = "Error: " + error;
    }

        #region Easy Tables

    public void Insert()
    {
        Highscore score = new Highscore();
        score.playername = "dimitris";
        score.score = UnityEngine.Random.Range(10,100);
        EasyTablesClient.Instance.Insert(score, insertResponse =>
        {
            if (insertResponse.Status == CallBackResult.Success)
            {
                string result = "Insert completed";
                if (Globals.DebugFlag) Debug.Log(result);
                StatusText.text = result;
            }
            else
            {
                ShowError(insertResponse.Exception.Message);
            }
        });
        StatusText.text = "Loading...";
    }

    public void SelectFiltered()
    {
        SelectFilteredExecute(false);
    }

    public void SelectFilteredCount()
    {
        SelectFilteredExecute(true);
    }

    private void SelectFilteredExecute(bool includeTotalCount)
    {
        string filterquery = "score gt 50 and startswith(playername,'dimi')";

        TableQuery tq = new TableQuery();
        tq.filter = filterquery;
        tq.orderBy = "score";
        tq.inlineCount = includeTotalCount;

        EasyTablesClient.Instance.SelectFiltered<Highscore>(tq, x =>
        {
            if (x.Status == CallBackResult.Success)
            {
                foreach (var item in x.Result.results)
                {
                    if (Globals.DebugFlag) Debug.Log(string.Format("ID is {0},score is {1},name is {2}", item.id, item.score, item.playername ));
                }
                if (includeTotalCount)
                {
                    StatusText.text = string.Format("Brought {0} rows out of {1}", x.Result.results.Count(), x.Result.count);
                }
                else
                {
                    StatusText.text = string.Format("Brought {0} rows", x.Result.results.Count());
                }
            }
            else
            {
                ShowError(x.Exception.Message);
            }
        });
        StatusText.text = "Loading...";
    }



    public void SelectByID()
    {
        EasyTablesClient.Instance.SelectByID<Highscore>("ecca86cb-8e35-47ac-8eef-74dc2ef87faa", x =>
        {
            if (x.Status == CallBackResult.Success)
            {
                Highscore hs = x.Result;
                if (Globals.DebugFlag) Debug.Log(hs.score);
                StatusText.text = "score of selected Highscore entry is " + hs.score;
            }
            else
            {
                ShowError(x.Exception.Message);
            }
        });
        StatusText.text = "Loading...";
    }

    public void UpdateSingle()
    {
        //Android disallows PATCH so we can't use the EasyTables solution
        //instead, we need an Easy API solution
        if (Application.platform == RuntimePlatform.Android)
        {
            CallUpdateForAndroid();
        }
        else
        {

            EasyTablesClient.Instance.SelectByID<Highscore>("bbd01bc4-52db-407d-83a4-d8b5422e300f", selectResponse =>
            {
                if (selectResponse.Status == CallBackResult.Success)
                {
                    Highscore hs = selectResponse.Result;
                    hs.score += 1;
                    EasyTablesClient.Instance.UpdateObject(hs, updateResponse =>
                    {
                        if (updateResponse.Status == CallBackResult.Success)
                        {
                            string msg = "object with id " + updateResponse.Result.id + " was updated";
                            if (Globals.DebugFlag) Debug.Log(msg);
                            StatusText.text = msg;
                        }
                        else
                        {
                            ShowError(updateResponse.Exception.Message);
                        }
                    });
                }
                else
                {
                    ShowError(selectResponse.Exception.Message);
                }
            });
            StatusText.text = "Loading...";
        }
    }

    public void DeleteByID()
    {
        EasyTablesClient.Instance.SelectByID<Highscore>("bbd01bc4-52db-407d-83a4-d8b5422e300f", selectResponse =>
        {
            if (selectResponse.Status == CallBackResult.Success)
            {
                Highscore hs = selectResponse.Result;
                EasyTablesClient.Instance.DeleteByID<Highscore>(hs.id, deleteResponse =>
                {
                    if (deleteResponse.Status == CallBackResult.Success)
                    {
                        string msg = "successfully deleted ID = " + hs.id;
                        if (Globals.DebugFlag) Debug.Log(msg);
                        StatusText.text = msg;
                    }
                    else
                    {
                        ShowError(deleteResponse.Exception.Message);
                    }
                });
            }
            else
            {
                ShowError(selectResponse.Exception.Message);
            }
        });
        StatusText.text = "Loading...";
    }
}

#endregion


//Helper class for Easy Tables
[Serializable()]
public class Highscore : AppServiceObjectBase
{
    public int score;
    public string playername;
}

