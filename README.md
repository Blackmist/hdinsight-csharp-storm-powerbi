This example demonstrates how you can send data to Microsoft Power BI Preview (powerbi.com,) from a Storm topology running on HDInsight. All the magic happens in the following code:

* **PowerBiBolt.cs**: Implements the Storm bolt, which sends data to Power BI

* **Data.cs**: Describes the data object/row that will be sent to Power BI

Otherwise, it's a pretty basic word count example. Power BI posting is handled through [PowerBi.Api.Client](https://github.com/Vtek/PowerBI.Api.Client).

## Prerequisites

* Visual Studio

* An Azure subscription

* An Azure Active Directory account with access to PowerBI.com

* The HDInsight tools for Visual Studio

* A Storm on HDInsight cluster

## To use this example

1. Follow the steps in the [Power BI quickstart](https://msdn.microsoft.com/en-US/library/dn931989.aspx) to sign up for Power BI.

2. Follow the steps in [Register an app](https://msdn.microsoft.com/en-US/library/dn877542.aspx) to create an application registration. This will be used when accessing the Power BI REST API.

    > [AZURE.IMPORTANT] Save the **Client ID** for the application registration.

3. Clone/fork/download a zip of this sample.

4. Open the solution in Visual Studio.

5. Right-click on the **WordCount** project and select properties. Here, you can change the name of the dataset that will be created in Power BI. By default, it is **Words**.

6. Open the **SCPHost.exe.config** file, find **<OAuth .../>** element, and set the following properties for it:

	* **Client**: the client ID for the application registration you created earlier.

	* **User**: the user name for the Azure Active Directory user that has access to Power BI.

	* **Password**: the user password.

6. Right-click on the **WordCount** project and select **Submit to Storm on HDInsight**. Select the Storm cluster name and deploy.

Once deployed, the topology will randomly emit sentences, count the occurrence of words, and emit the word and count to Power BI. If you visit http://powerbi.com and login as the user, you will see a new **Words** dataset, which can be used to create reports or a live dashboard. 

##Warning

The PowerBiBolt.cs file includes logic to create the dataset if it doesn't already exist. This works for this example, since the parallelism hint for the bolt is 1. If you create multiple instances of the bolt, and the multiple instances all try to create a new dataset, Power BI seems to actually allow you to create multiples, all with the same name.

So, for production use, you should create a standalone application that creates the dataset for you, before you deploy a topology with this bolt. Look at the **CreateDataset** project in this solution for an example of a standalone application to create the dataset.

##TODO

* Figure out some way to authenticate with a service account/application entry/something other than an AAD user.
