import React from "react";
import axios from "axios";
import { withMsal } from "@azure/msal-react";
import ApplicationList from "./ApplicationList";
import NewProcessForm from "./NewProcessForm";
import ApplicationDetail from "./ApplicationDetail";
import FilterCandidates from "./FilterCandidates";
import AlertMessage from "./AlertMessage";
import { tokens } from "@fluentui/react-components";

type HomePageState = {
    userId: string,
    userMail: string,
    username: string,
    isAuthenticated: boolean,
    showMain: boolean,
    showNewProcess: boolean,
    showAlert: boolean,
    showApplicationDetail: boolean,
    showFilterCandidates: boolean,
    file: File | null,
    nameLoadFile: string,
    dataTable: any,
    detailResponse: any,
    parsedFields: { [key: string]: string }
}

class HomePage extends React.Component<any, HomePageState> {
    constructor(props: any) {
        super(props);
        this.state = {
            userId: "",
            userMail: "",
            username: "",
            isAuthenticated: false,
            showMain: true,
            showNewProcess: false,
            showAlert: false,
            showApplicationDetail: false,
            showFilterCandidates: false,
            file: null,
            nameLoadFile: "",
            dataTable: [],
            detailResponse: {},
            parsedFields: {}
        };
        this.getApplications();
    }

    changeStatesTofalse = () => {
        this.setState({
            showMain: false,
            showNewProcess: false,
            showAlert: false,
            showApplicationDetail: false,
            showFilterCandidates: false
        });
    }

    showMain = () => {
        this.getApplications();
        this.changeStatesTofalse();
        this.setState({ showMain: true });
    }

    showNewProcess = () => {
        this.changeStatesTofalse();
        this.setState({ showNewProcess: true });
    }

    showApplicationDetail = async (id?: string) => {
        await this.getApplicationById(id);
        this.changeStatesTofalse();
        this.setState({ showApplicationDetail: true });
    }

    showFilterCandidates = async (id?: string) => {
        await this.getApplicationById(id);
        this.changeStatesTofalse();
        this.setState({ showFilterCandidates: true });
    }

    handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ nameLoadFile: event.target.value });
    };

    handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            this.setState({ file: e.target.files[0] });
        }
    };

    handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData();
        if (this.state.file) {
            formData.append('file', this.state.file);
        }
        formData.append('name', this.state.nameLoadFile);
        try {
            const response = await axios.post(process.env.REACT_APP_BACKEND_URI! + '/api/CreateApplication', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            });
            this.setState({ showAlert: true });
            console.log('Respuesta del servidor:', response.data);
        } catch (error) {
            console.error('Error al enviar el formulario:', error);
        }
    };

    getApplications = () => {
        axios.get(process.env.REACT_APP_BACKEND_URI! + '/api/GetApplications?page=1&pageSize=100', {
            headers: { 'Content-Type': 'application/json' }
        }).then(response => {
            this.setState({ dataTable: response.data.data });
        }).catch(error => {
            console.error('Error al obtener datos:', error);
        });
    }

    getApplicationById = async (id?: string) => {
        const response = await axios.get(process.env.REACT_APP_BACKEND_URI! + '/api/application/' + id, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        this.setState({
            detailResponse: response.data,
            parsedFields: response.data.data.fields ? JSON.parse(response.data.data.fields) : { mensaje: 'No existen campos' },
            showAlert: false
        });
    }

    render() {
        const { showMain, showNewProcess, showApplicationDetail, showFilterCandidates, showAlert, dataTable, detailResponse } = this.state;

        return (
            <div style={{ display: "grid", height: "100vh", backgroundColor: tokens.colorNeutralBackground4, gridTemplateColumns: "auto" }}>
                {showMain && <ApplicationList dataTable={dataTable} showApplicationDetail={this.showApplicationDetail} showFilterCandidates={this.showFilterCandidates} showNewProcess={this.showNewProcess} />}
                {showNewProcess && <NewProcessForm handleSubmit={this.handleSubmit} handleInputChange={this.handleInputChange} handleFileChange={this.handleFileChange} showAlert={showAlert} showMain={this.showMain} />}
                {showApplicationDetail && <ApplicationDetail detailResponse={detailResponse} showMain={this.showMain} />}
                {showFilterCandidates && <FilterCandidates detailResponse={detailResponse} showMain={this.showMain} />}
            </div>
        );
    }
}

export default withMsal(HomePage);
