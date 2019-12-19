/*
    <copyright file="router.tsx" company="Microsoft Corporation">
    Copyright (c) Microsoft Corporation. All rights reserved.
    </copyright>
*/

import React from 'react';
import { Route } from "react-router-dom";
import main from "../components/main";

const ReactRouter = () => {
    return (
        <React.Fragment>
            <Route path="/" component={main} />
        </React.Fragment>
    );
}
export default ReactRouter;
