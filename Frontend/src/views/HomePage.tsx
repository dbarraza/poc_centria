import React, { ChangeEvent,  createRef,  useEffect, useState  }  from "react";
import axios from "axios";
import { withMsal } from "@azure/msal-react";
import { tokens } from "@fluentui/react-components";

// This page state
type HomePageState = {
  userId: string,
  username: string,
  userMail: string,
  isAuthenticated: boolean
}

// Page component
class HomePage extends React.Component<any, HomePageState>  {

  apiClient = axios.create({
    baseURL:  process.env.REACT_APP_REDIRECT_URI ?? "", 
    headers: {
      "Content-type": "application/json",
      "userId":"",
      "userEmail":"",
      "Subscription-Key":  process.env.REACT_APP_SUBSCRIPTION_KEY ?? ""
    } 
  });

  constructor(props: any) {
    super(props);
    this.state = {      
      userId: "",    
      userMail: "",
      username: "",
      isAuthenticated: false,   
    };    
 
    this.apiClient.interceptors.request.use(
      config => {
        config.headers['userEmail'] =  this.state.userMail;
        config.headers['userId'] =  this.state.userId;
        return config;
        },
        error => {
            return Promise.reject(error);
        }        
    );
  }

  
  // Setups the callback for the login success event
  override  componentDidUpdate(): void {
    const msalInstance = this.props?.msalContext?.instance;
    const activeAccount = msalInstance?.getActiveAccount();

    if (activeAccount!=null && !this.state.isAuthenticated){    
      this.setState({
              username: activeAccount.name ?? "",
              userId: activeAccount?.localAccountId ?? "", 
              userMail: activeAccount.username ?? "",
              isAuthenticated: true
          }, () =>{      
        // Load data a this point
      });
    };
  }


  // Component render
  render() {
        return (
          
            <div style={{
              display: "grid",
              height: "100vh",
              backgroundColor: tokens.colorNeutralBackground4,
              gridTemplateColumns: "auto"
            }}>
              Hola mundo!
            </div>              
        );
      };
}


export default withMsal(HomePage);
