using System;
using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace FourPillarsAnalyser.ApiApp.Plugins.PersonalDetails;

public class PersonalDetailsPlugin
{
    [KernelFunction]
    [Description("Provides personal details based on the user inputs a list of specials from the menu.")]
    public string GetPersonalDetails(string year, string month, string day, string hour, string minute, string city, string gender) =>
        """
        Personal Details based on the provided information:

        Birth Date: : {year}-{month}-{day}
        Birth Time: {hour}:{minute}
        Birth City: {city}
        Gender: {gender}
        """;
}
