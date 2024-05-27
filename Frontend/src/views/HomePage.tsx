import React, {ChangeEvent, FormEvent} from "react";
import axios from "axios";
import {withMsal} from "@azure/msal-react";
import {tokens} from "@fluentui/react-components";
import AlertMessage from "../components/AlertMessage";

// This page state
type HomePageState = {
    userId: string,
    username: string,
    userMail: string,
    isAuthenticated: boolean,
    showTable: boolean,
    showForm: boolean,
    showAlert: boolean,
    showDetail: boolean,
    file: File | null,
    nameLoadFile: string,
    dataTable: any,
    detailResponse: any,
    parsedFields: { [key: string]: string }
}

// Page component
class HomePage extends React.Component<any, HomePageState>  {
    constructor(props: any) {
        super(props);
        this.state = {
            userId: "",
            userMail: "",
            username: "",
            isAuthenticated: false,
            showTable: true,
            showForm: false,
            showAlert: false,
            showDetail: false,
            file: null,
            nameLoadFile: "",
            dataTable: [],
            detailResponse: {},
            parsedFields: {}
        };
        this.getData();
    }

    public  getData = () => {
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

    public changeForm = () => {
        if(this.state.showForm){
            this.getData();
        }
        this.setState((prevState) => ({
            showForm: !prevState.showForm,
            showTable: !prevState.showTable,
            showAlert: false
        }));
    }

    public changeDetail = async (id?: string) => {
        if(!this.state.showDetail){
            const response = await axios.get(process.env.REACT_APP_BACKEND_URI! + '/api/application/' + id, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            });
            this.setState({
                detailResponse: response.data,
                parsedFields: ((response && response.data && response.data.data && response.data.data.fields !== undefined && response.data.data.fields !== null)) ? JSON.parse(response.data.data.fields) : {mensaje: 'No existen campos'},
                showAlert: false
            });
        }else{
            this.getData();
        }
        this.setState((prevState) => ({
            showDetail: !prevState.showDetail,
            showTable: !prevState.showTable
        }));

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
                {/* Formulario para crear un nuevo proceso de selcción */}
                {this.state.showForm && (
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
                                    <input type="text"  id="name" onChange={this.handleInputChange} className="file-input" required placeholder="Nombre"/>
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
                            <button onClick={this.changeForm}>Listado de aplicaciones</button>
                        </div>
                    </div>

                )}
                {/* Formulario para detalle */}
                {this.state.showDetail && (

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
                            <button onClick={() => this.changeDetail()}>Procesos de selección</button>
                        </div>
                    </div>

                )}
                {/* Pantalla Inicial - Lista de Procesos de selección */}
                {this.state.showTable && (

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
                                <th>Acciones</th>
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
                                        <button className="detalle-btn" onClick={() => this.changeDetail(data.id)}>Ver detalle</button>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                        <div className="btn-with-margin">
                            <button onClick={this.changeForm}>Nuevo Proceso</button>
                        </div>
                        <br/>
                    </div>
                )}
            </div>
        );
    };
}


export default withMsal(HomePage);
