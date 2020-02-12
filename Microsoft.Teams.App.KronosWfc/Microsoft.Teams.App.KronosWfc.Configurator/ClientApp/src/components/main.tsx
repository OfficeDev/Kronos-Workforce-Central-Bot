/*
    <copyright file="main.tsx" company="Microsoft Corporation">
    Copyright (c) Microsoft Corporation. All rights reserved.
    </copyright>
*/

import * as React from "react";
import { Menu, Container, Dropdown, Image, Card, Button, GridRow, Grid, GridColumn, Table, Header, Form, Icon, Segment, Dimmer, Loader, Label, Modal } from "semantic-ui-react";
import { ApplicationInsights, Trace, SeverityLevel } from '@microsoft/applicationinsights-web';
import { ReactPlugin, withAITracking } from '@microsoft/applicationinsights-react-js';
import { createBrowserHistory } from "history";
import { getToken, authContext } from '../adalConfig';

const browserHistory = createBrowserHistory({ basename: '' });

interface IProps { }
var reactPlugin = new ReactPlugin();

/** State interface. */
interface IState {
    isAuthenticated: boolean,
    selectedTenant: any,
    tenantArray: Array<any>,
    loginDetails: { Username: any, Password: any },
    tenantPaycodes: Array<any>,
    tenantInfo: any,
    tenantSuperuserInfo: any,
    paycodeMapping: { PaycodeType: any, PaycodeName: any },
    tenantInfoLoading: boolean,
    superuserConfigLoading: boolean,
    tenantPaycodesLoading: boolean,
    modalStateOpen: boolean,
    editViewBtnState: boolean,
    showPaycodeAddError: boolean,
    showMandatoryFieldsErrorMessage: boolean,
    showPaycodeErrorMessage: boolean
};

/** AddFavourites component. */
class main extends React.Component<IProps, IState> {
    state: IState;
    token: any = null;
    bearer:string = "";
    /** Instrumentation key*/
    telemetry: any = undefined;
    appInsights: ApplicationInsights;
    paycodeTypes: Array<any> = [
        {
            key: 'Overtime',
            text: 'Overtime',
            value: 'Overtime'
        },
        {
            key: 'Regular',
            text: 'Regular',
            value: 'Regular'
        },
        {
            key: 'Sick',
            text: 'Sick',
            value: 'Sick'
        },
        {
            key: 'Vacation',
            text: 'Vacation',
            value: 'Vacation'
        }
    ]


    /**
     * Contructor to initialize component.
     * @param props Props of component.
     */
    constructor(props: IProps) {
        super(props);
        this.state = {
            isAuthenticated: false,
            selectedTenant: null,
            tenantArray: [],
            loginDetails: { Username: "", Password: "" },
            tenantPaycodes: [],
            tenantInfo: {},
            tenantSuperuserInfo: {},
            paycodeMapping: { PaycodeName: "", PaycodeType: undefined },
            tenantInfoLoading: false,
            superuserConfigLoading: false,
            tenantPaycodesLoading: false,
            modalStateOpen: false,
            editViewBtnState: false,
	    showPaycodeAddError: false,
            showPaycodeErrorMessage: false,
            showMandatoryFieldsErrorMessage: false
        };
        this.appInsights = new ApplicationInsights({
            config: {
                instrumentationKey: this.telemetry,
                extensions: [reactPlugin],
                extensionConfig: {
                    [reactPlugin.identifier]: { history: browserHistory }
                }
            }
        });
        //this.appInsights.loadAppInsights();
        this.bearer = `Bearer ${getToken()}`;
    }

    async componentDidMount() {
        this.getTenantInfo();
        this.getPaycodes();
        this.getSuperuserConfig();
    }

    /** Get superuser config information for selected tenant. */
    async getSuperuserConfig() {
        this.setState({ superuserConfigLoading: true });
        let request = new Request("/api/TenantConfig/GetSuperuserConfig", {
            headers: new Headers({
                "Authorization": this.bearer
            })
        });

        const res = await fetch(request);
        this.setState({ superuserConfigLoading: false });
        if (res.status === 401) {
            //this.appInsights.trackTrace({ message: `User ${this.userObjectId} is unauthorized!`, severityLevel: SeverityLevel.Warning });
            this.setState({ isAuthenticated: false });
            this.logout();
        }
        else if (res.status === 200) {
            const response = await res.json();
            if (response !== null) {
                this.setState({ tenantSuperuserInfo: response, isAuthenticated:true });
            }
            else {
                //this.setState({ loadingFavList: false });
                //this.setMessage("Unable to fetch favourite rooms. Try again.", ErrorMessageRedColor, false);
            }
        }
        else {
            //this.appInsights.trackTrace({ message: `'TopFiveRoomsAsync' - Request failed:${res.status}`, severityLevel: SeverityLevel.Warning });
            //this.setMessage("Server error :" + res.status, ErrorMessageRedColor, false);
        }
    }

    /** Get all tenants from table storage. */
    async getPaycodes() {
        this.setState({ tenantPaycodesLoading: true });
        let request = new Request("/api/TenantConfig/GetPaycodeMapping", {
            headers: new Headers({
                "Authorization": this.bearer
            })
        });

        const res = await fetch(request);
        this.setState({ tenantPaycodesLoading: false });
        if (res.status === 401) {
            //this.appInsights.trackTrace({ message: `User ${this.userObjectId} is unauthorized!`, severityLevel: SeverityLevel.Warning });
            this.setState({ isAuthenticated: false });
            this.logout();
        }
        else if (res.status === 200) {
            const response = await res.json();
            if (response !== null) {
                this.setState({ tenantPaycodes: response, isAuthenticated: true });
            }
            else {
                //this.setState({ loadingFavList: false });
                //this.setMessage("Unable to fetch favourite rooms. Try again.", ErrorMessageRedColor, false);
            }
        }
        else {
            //this.appInsights.trackTrace({ message: `'TopFiveRoomsAsync' - Request failed:${res.status}`, severityLevel: SeverityLevel.Warning });
            //this.setMessage("Server error :" + res.status, ErrorMessageRedColor, false);
        }
    }

    /** Get information of selected tenant from table storage. */
    async getTenantInfo() {
        this.setState({ tenantInfoLoading: true });
        let request = new Request("/api/TenantConfig/GetTenantInfo", {
            headers: new Headers({
                "Authorization": this.bearer
            })
        });

        const res = await fetch(request);
        this.setState({ tenantInfoLoading: false });
        if (res.status === 401) {
            //this.appInsights.trackTrace({ message: `User ${this.userObjectId} is unauthorized!`, severityLevel: SeverityLevel.Warning });
            this.setState({ isAuthenticated: false });
            this.logout();
        }
        else if (res.status === 200) {
            const response = await res.json();
            if (response !== null) {
                this.setState({ tenantInfo: response, isAuthenticated: true});
            }
            else {
                //this.setState({ loadingFavList: false });
                //this.setMessage("Unable to fetch favourite rooms. Try again.", ErrorMessageRedColor, false);
            }
        }
        else {
            //this.appInsights.trackTrace({ message: `'TopFiveRoomsAsync' - Request failed:${res.status}`, severityLevel: SeverityLevel.Warning });
            //this.setMessage("Server error :" + res.status, ErrorMessageRedColor, false);
        }
    }

    /** Save tenant info */
    setTenantInfo = async () => {
        this.setState({ tenantInfoLoading: true });
        let tenantInfo = this.state.tenantInfo;
        tenantInfo.PartitionKey = "msteams";
        const res = await fetch("/api/TenantConfig/SetTenantInfo", {
            method: "POST",
            headers: new Headers({
                "Authorization": this.bearer,
                "Content-Type": "application/json",
            }),
            body: JSON.stringify(tenantInfo)
        });
        this.setState({ tenantInfoLoading: false });
        let response = res;
        if (res.status === 401) {
            //this.appInsights.trackTrace({ message: `User ${this.userObjectId} is unauthorized!`, severityLevel: SeverityLevel.Warning });
            this.setState({ isAuthenticated: false });
            this.logout();
        }
        else if (res.status === 200) {
            
        }
        else {
            //this.appInsights.trackTrace({ message: `'TopFiveRoomsAsync' - Request failed:${res.status}`, severityLevel: SeverityLevel.Warning });
            //this.setMessage("Server error :" + res.status, ErrorMessageRedColor, false);
        }  
    }

    /** Save paycodes */
    setPaycodes = async () => {
        this.setState({ tenantPaycodesLoading: true });
        const res = await fetch("/api/TenantConfig/SetPaycodeMapping", {
            method: "POST",
            headers: new Headers({
                "Authorization": this.bearer,
                "Content-Type": "application/json",
            }),
            body: JSON.stringify(this.state.tenantPaycodes)
        });
        this.setState({ tenantPaycodesLoading: false });
        let response = res;
        if (res.status === 401) {
            //this.appInsights.trackTrace({ message: `User ${this.userObjectId} is unauthorized!`, severityLevel: SeverityLevel.Warning });
            this.setState({ isAuthenticated: false });
            this.logout();
        }
        else if (res.status === 200) {

        }
        else {
            //this.appInsights.trackTrace({ message: `'TopFiveRoomsAsync' - Request failed:${res.status}`, severityLevel: SeverityLevel.Warning });
            //this.setMessage("Server error :" + res.status, ErrorMessageRedColor, false);
        }
    }

    /** Save superuser config */
    setSuperuserConfig = async () => {
        this.setState({ superuserConfigLoading: true });
        const res = await fetch("/api/TenantConfig/SetSuperuserConfig", {
            method: "POST",
            headers: new Headers({
                "Authorization": this.bearer,
                "Content-Type": "application/json",
            }),
            body: JSON.stringify(this.state.tenantSuperuserInfo)
        });
        this.setState({ superuserConfigLoading: false });
        let response = res;
        if (res.status === 401) {
            //this.appInsights.trackTrace({ message: `User ${this.userObjectId} is unauthorized!`, severityLevel: SeverityLevel.Warning });
            this.setState({ isAuthenticated: false });
            this.logout();
        }
        else if (res.status === 200) {

        }
        else {
            //this.appInsights.trackTrace({ message: `'TopFiveRoomsAsync' - Request failed:${res.status}`, severityLevel: SeverityLevel.Warning });
            //this.setMessage("Server error :" + res.status, ErrorMessageRedColor, false);
        }
    }

    logout = () => {
        authContext.logOut();
    }

    onPaycodeTypeSelection = (value: any) => {
        this.setState({ paycodeMapping: { ...this.state.paycodeMapping, PaycodeType: value } })
    }

    deletePaycodeMapping = (index: number) => {
        let paycodes = this.state.tenantPaycodes;
        paycodes.splice(index, 1);
        this.setState({ tenantPaycodes: paycodes })
    }

    addPaycodeMapping = async () => {
        this.setState({ showMandatoryFieldsErrorMessage: false });
        this.setState({ showPaycodeErrorMessage: false });
        var paycode = this.state.paycodeMapping;
        var tenantPaycodes = this.state.tenantPaycodes;
        var existingPaycode = tenantPaycodes.filter(function (tenantPaycode) { return tenantPaycode.PayCodeType === paycode.PaycodeType && tenantPaycode.PayCodeName == paycode.PaycodeName });

        var endpoint = this.state.tenantInfo.EndpointUrl;
        var tenantId = this.state.tenantInfo.RowKey;
        var userName = this.state.tenantSuperuserInfo.SuperUsername;
        var password = this.state.tenantSuperuserInfo.SuperUserPassword;

        if (existingPaycode.length > 0) {
            this.setState({ showPaycodeErrorMessage: true });
        }
        else if (tenantId === undefined || tenantId === "" 
	|| endpoint === undefined || endpoint === "" 
	|| userName === undefined || userName === "" 
	|| password === undefined || password === "") {
            this.setState({ showMandatoryFieldsErrorMessage: true });
        }
        else {
            const res = await fetch("/api/TenantConfig/GetValuesFromKronos?endpoint=" + endpoint + "&tenantId=" + tenantId + "&username=" + userName
                + "&password=" + password, {
                method: "GET",
                headers: new Headers({
                    "Authorization": this.bearer,
                    "Content-Type": "application/json",
                }),
                //body: JSON.stringify(this.state.tenantInfo.EndpointUrl)
            });
            if (res.status === 200) {
                const response = await res.json();
                
                if (response.result.indexOf(this.state.paycodeMapping.PaycodeName) > -1) {
                    if (this.state.paycodeMapping.PaycodeName !== undefined && this.state.paycodeMapping.PaycodeName !== "") {
                        if (this.state.paycodeMapping.PaycodeType !== undefined && this.state.paycodeMapping.PaycodeType !== null) {
                            let paycodes = this.state.tenantPaycodes;
                            paycodes.push({ PartitionKey: 'msteams', RowKey: this.state.tenantInfo.RowKey + '$' + this.state.paycodeMapping.PaycodeType + '$' + this.state.paycodeMapping.PaycodeName.replace(/\s/g, ''), PayCodeName: this.state.paycodeMapping.PaycodeName, PayCodeType: this.state.paycodeMapping.PaycodeType });
                            this.setState({ tenantPaycodes: paycodes, paycodeMapping: { PaycodeName: "", PaycodeType: "" }, showPaycodeAddError: false });
                        }
                    }
                }
                else {
                    this.setState({ showPaycodeAddError: true });
                }
            }
        }
    }

    close = () => this.setState({ modalStateOpen: false })

    toggleViewMode = () => {
        this.setState({ editViewBtnState: this.state.editViewBtnState == true ? false : true });
    }

    render()
    {
        const renderDetails = () => {
            return (
                <Container>
                    <Container textAlign='right'>
                        <Button secondary style={{ marginTop: '6em'}} type='button'
                            onClick={() => {
                                this.toggleViewMode();
                            }}>
                            {this.state.editViewBtnState == true ? 'View' : 'Edit'}
                        </Button>
                    </Container>
                    <Grid style={{ marginTop: '2em', border: '1px solid lightgrey', borderRadius: '5px' }}>
                        <GridRow columns={2}>
                            <GridColumn>
                                <Card style={{ border: 'none', boxShadow: 'none'  }} fluid>
                                    <Dimmer active={this.state.tenantInfoLoading}>
                                        <Loader content='Loading' />
                                    </Dimmer>
                                    <Card.Content style={{ border: 'none' }}>
                                        <Card.Header>Tenant configuration</Card.Header>
                                        <Card.Description style={{ marginTop: '2em' }}>
                                            <Form>
                                                <Form.Field>
                                                    <label>Tenant Id</label>
                                                    <input disabled={!this.state.editViewBtnState} placeholder='Enter valid tenant id' value={this.state.tenantInfo.RowKey} onChange={(e) => { this.setState({ tenantInfo: { ...this.state.tenantInfo, RowKey: e.target.value } }) }} />
                                                </Form.Field>
                                                <Form.Field>
                                                    <label>Kronos endpoint URL</label>
                                                    <input disabled={!this.state.editViewBtnState} placeholder='Enter kronos endpoint url' value={this.state.tenantInfo.EndpointUrl} onChange={(e) => { this.setState({ tenantInfo: { ...this.state.tenantInfo, EndpointUrl: e.target.value } }) }}/>
                                                </Form.Field>
                                            </Form>
                                        </Card.Description>
                                    </Card.Content>
                                </Card>
                            </GridColumn>
                            <GridColumn>
                                <Card style={{ border: 'none',boxShadow:'none' }} fluid>
                                    <Dimmer active={this.state.superuserConfigLoading}>
                                        <Loader content='Loading' />
                                    </Dimmer>
                                    <Card.Content style={{ border: 'none' }}>
                                        <Card.Header>Kronos Superuser configuration</Card.Header>
                                        <Card.Description style={{ marginTop: '2em' }}>
                                            <Form>
                                                <Form.Field>
                                                    <label>Username</label>
                                                    <input disabled={!this.state.editViewBtnState} placeholder='Enter superuser username' value={this.state.tenantSuperuserInfo.SuperUsername} onChange={(e) => { this.setState({ tenantSuperuserInfo: { ...this.state.tenantSuperuserInfo, SuperUsername: e.target.value } }) }}/>
                                                </Form.Field>
                                                <Form.Field>
                                                    <label>Password</label>
                                                    <input disabled={!this.state.editViewBtnState} type="password" placeholder='Enter superuser password' value={this.state.tenantSuperuserInfo.SuperUserPassword} onChange={(e) => { this.setState({ tenantSuperuserInfo: { ...this.state.tenantSuperuserInfo, SuperUserPassword: e.target.value } }) }}/>
                                                </Form.Field>
                                            </Form>
                                        </Card.Description>
                                    </Card.Content>
                                </Card>
                            </GridColumn>
                        </GridRow>
                        <GridRow>
                            <GridColumn>
                                <Card style={{ border: 'none', boxShadow: 'none'  }} fluid>
                                    <Dimmer active={this.state.tenantPaycodesLoading}>
                                        <Loader content='Loading' />
                                    </Dimmer>
                                    <Card.Content style={{ border: 'none' }}>
                                        <Card.Header>Paycode configuration</Card.Header>
                                        <Card.Description style={{ marginTop: '2em' }}>
                                            <Grid>
                                                <GridRow columns={3}>
                                                    <GridColumn width={5}>
                                                        <Form>
                                                            <Form.Field>
                                                                <label>Select paycode type</label>
                                                                <Dropdown
                                                                    disabled={!this.state.editViewBtnState}
                                                                    placeholder='Select paycode type'
                                                                    fluid
                                                                    selection
                                                                    options={this.paycodeTypes}
                                                                    onChange={(e, { value }) => this.onPaycodeTypeSelection(value)}
                                                                    value={this.state.paycodeMapping.PaycodeType}
                                                                />
                                                            </Form.Field>
                                                            <Form.Field>
                                                                <label>Paycode name</label>
                                                                <input disabled={!this.state.editViewBtnState} placeholder='Enter paycode name' value={this.state.paycodeMapping.PaycodeName} onChange={(e) => { this.setState({ paycodeMapping: { ...this.state.paycodeMapping, PaycodeName: e.target.value } }) }} />
                                                            </Form.Field>
                                                            <Form.Field>
                                                                <label style={{ display: this.state.showPaycodeAddError ? 'block' : 'none', color:"red" }}>Entered Paycode Name is not available in Kronos.</label>
								{this.state.showMandatoryFieldsErrorMessage && <label style={{ color: 'red' }}>Please fill all the fields under Tenant and Kronos Superuser configuration.</label>}
                                                                {this.state.showPaycodeErrorMessage && <label style={{ color: 'red' }}>Paycode type and Paycode name is already available.</label>}
                                                            </Form.Field>
                                                        </Form>
                                                    </GridColumn>
                                                    <GridColumn width={3}>
                                                        <Button disabled={!this.state.editViewBtnState} style={{marginTop:'4em'}} secondary type='submit' onClick={() => this.addPaycodeMapping()}>Add</Button>
                                                    </GridColumn>
                                                    <GridColumn width={6}>
                                                        <div style={{ maxHeight: '20em', overflowY: 'scroll' }}>
                                                            <Table singleLine fluid wid>
                                                                <Table.Header>
                                                                    <Table.Row>
                                                                        <Table.HeaderCell>#</Table.HeaderCell>
                                                                        <Table.HeaderCell>Paycode type</Table.HeaderCell>
                                                                        <Table.HeaderCell>Paycode name</Table.HeaderCell>
                                                                        <Table.HeaderCell></Table.HeaderCell>
                                                                    </Table.Row>
                                                                </Table.Header>

                                                                <Table.Body>
                                                                    {
                                                                        this.state.tenantPaycodes.map
                                                                            ((item: any, i: number) => {
                                                                                return (
                                                                                    <Table.Row key={'row' + i.toString()}>
                                                                                        <Table.Cell>{i + 1}</Table.Cell>
                                                                                        <Table.Cell>{item.PayCodeType}</Table.Cell>
                                                                                        <Table.Cell>{item.PayCodeName}</Table.Cell>
                                                                                        <Table.Cell><Button disabled={!this.state.editViewBtnState} size='mini' key={'delete' + i.toString()} type='submit' onClick={() => this.deletePaycodeMapping(i)}>Delete</Button> </Table.Cell>
                                                                                    </Table.Row>
                                                                                )
                                                                            }
                                                                            )
                                                                    }
                                                                </Table.Body>
                                                            </Table>
                                                        </div>                                                        
                                                    </GridColumn>
                                                </GridRow>
                                            </Grid>
                                        </Card.Description>
                                    </Card.Content>
                                    <Card.Content extra>
                                        <Button disabled={!this.state.editViewBtnState} floated="right" secondary type='submit'
                                            onClick={() => {
                                                var endpoint = this.state.tenantInfo.EndpointUrl;
                                                var tenantId = this.state.tenantInfo.RowKey;
                                                var userName = this.state.tenantSuperuserInfo.SuperUsername;
                                                var password = this.state.tenantSuperuserInfo.SuperUserPassword;
                                                if (tenantId === undefined || tenantId === ""
                                                    || endpoint === undefined || endpoint === ""
                                                    || userName === undefined || userName === ""
                                                    || password === undefined || password === "") {
                                                    this.setState({ showMandatoryFieldsErrorMessage: true });
                                                }
                                                else {
                                                    this.setState({ showMandatoryFieldsErrorMessage: false });
                                                    this.setState({ showPaycodeErrorMessage: false });
                                                    this.setState({ showPaycodeAddError: false });
                                                    this.setState({ modalStateOpen: true });
                                                }
                                            }
                                            }>
                                            Submit
                                        </Button>
                                        <Modal size="mini" open={this.state.modalStateOpen}>
                                            <Modal.Header>Save Changes</Modal.Header>
                                            <Modal.Content>
                                                <p>Are you sure you want to save your changes</p>
                                            </Modal.Content>
                                            <Modal.Actions>
                                                <Button onClick={this.close} secondary>No</Button>
                                                <Button
                                                    secondary
                                                    onClick={() => {
                                                        this.setTenantInfo();
                                                        this.setSuperuserConfig()
                                                        this.setPaycodes();
                                                        this.toggleViewMode();
                                                        this.close();
                                                    }}
                                                >Yes</Button>
                                            </Modal.Actions>
                                        </Modal>
                                    </Card.Content>
                                </Card>
                            </GridColumn>
                        </GridRow>
                    </Grid>
                </Container>
                );
        }
        if (this.state.isAuthenticated) {
            return (
                <Container fluid>
                    <Menu fixed='top' inverted>
                        <Container>
                            <Menu.Item as='a' header>
                                Kronos
					    </Menu.Item>

                            <Menu.Menu position='right'>
                                <Menu.Item as='a' onClick={() => this.logout()}>Logout</Menu.Item>
                            </Menu.Menu>
                        </Container>
                    </Menu>
                    {renderDetails()}
                </Container>
            )
        }
        else {
            return (
                <Container fluid>
                    <div>Loading, Please wait..</div>
                </Container>
            )
        }
    }
}

export default withAITracking(reactPlugin, main);
