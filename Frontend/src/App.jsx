import "./App.css";
import { Route, Routes, useNavigate  } from "react-router-dom";
import { FluentProvider, teamsLightTheme  } from "@fluentui/react-components";
import HomePage  from "./views/HomePage";
import { PageLayout } from './components/PageLayout';
import { MsalProvider, useMsal } from '@azure/msal-react';

const Pages = () => {
  const { instance  } = useMsal();

  return (
      <Routes>
          <Route exact path="/" element={<HomePage />} />          
      </Routes>
  );
};

const App = ({ instance }) => {
  return (
    <div className="App">
      <FluentProvider theme={teamsLightTheme}>
        <MsalProvider instance={instance}>
          <PageLayout>
            <Pages />
          </PageLayout>
        </MsalProvider>
      </FluentProvider>
    </div>
  );
};
export default App;
