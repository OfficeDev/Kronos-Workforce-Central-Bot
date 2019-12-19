/*
    <copyright file="index.tsx" company="Microsoft Corporation">
    Copyright (c) Microsoft Corporation. All rights reserved.
    </copyright>
*/

import React from 'react';
import ReactDOM from 'react-dom';
import 'semantic-ui-css/semantic.min.css';
import { BrowserRouter as Router } from 'react-router-dom';
import ReactRouter from './router/router';
import { runWithAdal } from 'react-adal';
import { authContext } from './adalConfig';

const DO_NOT_LOGIN = false;
runWithAdal(authContext, () => {
    ReactDOM.render(
        <Router>
            <ReactRouter />
        </Router>, document.getElementById('root'));
}, DO_NOT_LOGIN);