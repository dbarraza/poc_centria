import React, { ChangeEvent, FormEvent } from "react";
import axios from "axios";
import { withMsal } from "@azure/msal-react";
import { tokens } from "@fluentui/react-components";
import AlertMessage from "../components/AlertMessage";

// This page state
type HomePageState = {
    userId: string,
    username: string,
    userMail: string,
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

// Page component
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

    public changeStatesTofalse = () => {
        this.setState({
            showMain: false,
            showNewProcess: false,
            showAlert: false,
            showApplicationDetail: false,
            showFilterCandidates: false
        });
    }

    public showMain = () => {
        alert('showMain');
        this.getApplications();
        this.changeStatesTofalse();
        this.setState((prevState) => ({
            showMain: true
        }));
    }

    public showNewProcess = () => {
        this.changeStatesTofalse();
        this.setState((prevState) => ({
            showNewProcess: true
        }));
    }

    public showApplicationDetail = async (id?: string) => {
        await this.getApplicationById(id);
        this.changeStatesTofalse();
        this.setState((prevState) => ({
            showApplicationDetail: true
        }));
    }

    public showFilterCandidates = async (id?: string) => {
        await this.getApplicationById(id);
        this.changeStatesTofalse();
        this.setState((prevState) => ({
            showFilterCandidates: true
        }));
    }

    public handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
        this.setState({ nameLoadFile: event.target.value });
    };

    public handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            this.setState({ file: e.target.files[0] });
        }
    };

    public handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        const formData = new FormData();
        if (this.state.file) {
            formData.append('file', this.state.file);
        }
        formData.append('name', this.state.nameLoadFile);
        debugger
        try {
            const response = await axios.post(process.env.REACT_APP_BACKEND_URI! + '/api/CreateApplication', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            });
            this.setState({
                showAlert: true
            });
            console.log('Respuesta del servidor:', response.data);
        } catch (error) {
            console.error('Error al enviar el formulario:', error);
        }
    };

    public getApplications = () => {
        axios.get(process.env.REACT_APP_BACKEND_URI! + '/api/GetApplications?page=1&pageSize=100', {
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(response => {
            this.setState({ dataTable: response.data.data });
        }).catch(error => {
            console.error('Error al obtener datos:', error);
        });
    }

    public getApplicationById = async (id?: string) => {
        const response = await axios.get(process.env.REACT_APP_BACKEND_URI! + '/api/application/' + id, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
        this.setState({
            detailResponse: response.data,
            parsedFields: ((response && response.data && response.data.data && response.data.data.fields !== undefined && response.data.data.fields !== null)) ? JSON.parse(response.data.data.fields) : { mensaje: 'No existen campos' },
            showAlert: false
        });
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
                {/* Pantalla Inicial - Lista de Procesos de selección */}
                {this.state.showMain && (

                    <div className="component-container">

                        <div className="divTittle">
                            <h1>Procesos de selección</h1>
                        </div>
                        <table className="lista">
                            <thead>
                                <tr>
                                    <th>Id</th>
                                    <th>Nombre</th>
                                    <th>Fecha Creación</th>
                                    <th>Estado</th>
                                    <th>Archivo</th>
                                    <th>-</th>
                                    <th>-</th>
                                </tr>
                            </thead>
                            <tbody>
                                {this.state.dataTable.map((data: any, index: number) => (
                                    <tr key={index}>
                                        <td>{data.id}</td>
                                        <td>{data.name}</td>
                                        <td>{data.createdAt}</td>
                                        <td>{data.status}</td>
                                        <td><a href={data.excelUrl} target="_blank" rel="noopener noreferrer">Ver archivo</a></td>
                                        <td>
                                            <button className="detalle-btn" onClick={() => this.showApplicationDetail(data.id)}>Ver detalle</button>
                                        </td>
                                        <td>
                                            <button className="detalle-btn" onClick={() => this.showFilterCandidates(data.id)}>Filtrar Candidatos</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        <div className="btn-with-margin">
                            <button onClick={this.showNewProcess}>Nuevo Proceso</button>
                        </div>
                        <br />
                    </div>
                )}
                {/* Formulario para crear un nuevo proceso de selección */}
                {this.state.showNewProcess && (
                    <div className="component-container">
                        {this.state.showAlert && (<AlertMessage
                            message="¡El archivo se cargo con exitosamente!"
                        />)}
                        {!this.state.showAlert && (<div>
                            <div className="divTittle">
                                <h1>Nuevo Proceso de selección</h1>
                            </div>
                            <h3>Por favor ingrese el nombre de la acción y el archivo</h3>
                            <form onSubmit={this.handleSubmit} encType="multipart/form-data">
                                <div>
                                    <input type="text" id="name" onChange={this.handleInputChange} className="file-input" required placeholder="Nombre" />
                                </div>
                                <div>
                                    <input type="file" id="file" onChange={this.handleFileChange} className="file-input" required />
                                </div>
                                <div>
                                    <button type="submit">Enviar</button>
                                </div>
                            </form>
                        </div>)}
                        <div>
                            <button onClick={this.showMain}>Listado de aplicaciones</button>
                        </div>
                    </div>

                )}
                {/* Formulario para detalle de un proceso de selección */}
                {this.state.showApplicationDetail && (

                    <div className="component-container">
                        <div className="divTittle">
                            <h1>Detalles de la acción</h1>
                        </div>
                        <div className="field">
                            <span className="field-label">ID:</span>
                            <span className="field-value">{this.state.detailResponse.data.id}</span>
                        </div>
                        <div className="field">
                            <span className="field-label">Nombre:</span>
                            <span className="field-value">{this.state.detailResponse.data.name}</span>
                        </div>
                        <div className="field">
                            <span className="field-label">Estado:</span>
                            <span className="field-value">{this.state.detailResponse.data.status}</span>
                        </div>
                        <div className="field">
                            <span className="field-label">Fecha de creación:</span>
                            <span className="field-value">{this.state.detailResponse.data.createdAt}</span>
                        </div>
                        <div className="field">
                            <span className="field-label">URL:</span>
                            <a className="field-link" href={this.state.detailResponse.data.excelUrl}>{this.state.detailResponse.data.excelUrl}</a>
                        </div>
                        <div>
                            <button onClick={() => this.showMain()}>Procesos de selección</button>
                        </div>
                    </div>

                )}
                {/* Formulario para filtrar candidatos */}
                {this.state.showFilterCandidates && (
                    <div className="component-container">

                        <div className="divTittle">
                            <h1>Filtrado de candiataos</h1>
                        </div>

                        <div className="field">
                            <span className="field-label">ID:</span>
                            <span className="field-value">{this.state.detailResponse.data.id}</span>
                        </div>
                        <div className="field">
                            <span className="field-label">Nombre:</span>
                            <span className="field-value">{this.state.detailResponse.data.name}</span>
                        </div>

                        <div>
                            <button onClick={() => this.showMain()}>Procesos de selección</button>
                        </div>

                    </div>
                )}
            </div>
        );
    };
}


export default withMsal(HomePage);
