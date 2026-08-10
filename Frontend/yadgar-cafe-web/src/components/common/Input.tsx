import "./Input.css";
import type { ReactNode } from "react";

type Props = {
    label: string;
    icon?: ReactNode;
    type?: string;
    placeholder?: string;
}

export default function Input({
    label,
    icon,
    type = "text",
    placeholder
}: Props) {

    return (

        <div className="input-group">

            <label>

                {icon}

                <span>{label}</span>

            </label>

            <input
                type={type}
                placeholder={placeholder}
            />

        </div>

    );

}