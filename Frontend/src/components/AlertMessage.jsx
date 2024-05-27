import React from 'react';
import {MessageBar} from "@fluentui/react-components";

const AlertMessage = ({ message }) => {

    const handleOkClick = () => {
       console.log('redirect');
    };

    return (
        <MessageBar
            isMultiline={false}
            onDismiss={handleOkClick}
            dismissButtonAriaLabel="Close"
        >
            {message}
        </MessageBar>
    );
};

export default AlertMessage;
