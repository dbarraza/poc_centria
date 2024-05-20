import React, { useEffect, useState } from "react";
import {AuthenticatedTemplate,useMsal} from "@azure/msal-react";
import {loginRequest} from "../config/authConfig";

// Base page layout
export const PageLayout = (props) => {

    const { instance, inProgress  } = useMsal();    
    const activeAccount = instance.getActiveAccount();
      
    const handleLoginRedirect = () => {
        instance.loginRedirect(loginRequest).catch((error) => console.log(error));
    };

    const handleLogoutRedirect = () => {
        instance.logoutRedirect().catch((error) => console.log(error));
    };


    useEffect(() => {
        if (activeAccount ==null){
           handleLoginRedirect();
        }
        else
        {
            console.log("User is logged in") ;       
        }
    });

    return (
        <>           
            {props.children}
        </>
    );
}